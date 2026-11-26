#!/usr/bin/env python3
"""Eyes: The Horror Game - restored multiplayer backend (private).

Implements the protocol the 8.3.0 client speaks, recovered from the ARM build:
  POST /api/v1/login                Basic credential check (accounts are created on first use)
  POST /api/v1/find                 random matchmaking, returns MultiGame
  POST /api/v1/create               friend game, returns GameWithToken
  GET  /api/v1/find?token=XXXXX     join a friend game, returns MultiGame
  GET  /api/v1/ping                 health check, also carries the free keys flag
  GET  /api/v1/news                 news published from the admin panel
  GET  /admin                       web admin panel (HTTP Basic, user "admin")
  WS   /ws/v1/diag                  connection check for the client's server test
  WS   /ws/v1/{gameId}/join/{playerId}   game relay
Anything else answers 200 {} so optional features degrade quietly.

Standard library only. Python 3.8+.
  python eyes_server.py --port 8080 --data eyes_server.db
  python eyes_server.py --port 443 --cert fullchain.pem --key privkey.pem
The admin password comes from --admin-password or EYES_ADMIN_PASSWORD; without one a random password is
generated, stored in the database and printed to the log once.
"""
import argparse
import asyncio
import base64
import hashlib
import json
import logging
import os
import secrets
import sqlite3
import ssl
import struct
import time
import urllib.parse

log = logging.getLogger("eyes")

WS_GUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
MIN_CLIENT_VERSION = 4
MAX_HTTP_BODY = 64 * 1024
MAX_WS_FRAME = 256 * 1024
RECONNECT_GRACE_SECONDS = 20
WAITING_ROOM_TTL_SECONDS = 600
FINISHED_ROOM_TTL_SECONDS = 120
TOKEN_ALPHABET = "0123456789"  # the client's invitation input field accepts digits only

# Daily challenges built into the client: id -> (kind, parameter, goal, title). Ids match its catalog.
DAILY_BUILTIN = {
    1: (0, 50, 50, "Daily collection: collect 50 bags"),
    2: (1, 2, 1, "Door limit: Mansion, each door at most twice"),
    3: (2, 500, 1, "Win in 500 steps"),
    4: (3, 4, 4, "Hospital potion discoveries"),
    5: (4, 1, 1, "Eyes before bags"),
    6: (5, 300, 1, "Beat the clock: win in 5 minutes"),
    7: (6, 1, 1, "Family matters: portrait puzzle"),
    8: (7, 1, 1, "Win against a stranger (VS)"),
    9: (8, 1, 1, "Silent escape"),
    10: (9, 4, 1, "Pumpkin Party"),
    11: (10, 5, 1, "Feed Good Boy"),
    12: (11, 1, 1, "Together in Fear (co-op random)"),
    13: (12, 1, 1, "Bound by Terror (co-op friend)"),
    14: (13, 1, 1, "No Mercy for Friends (VS friend)"),
    15: (14, 24, 1, "Explore the Mansion"),
    16: (15, 25, 1, "Explore the School"),
    17: (16, 17, 1, "Explore the Hospital"),
}
DAILY_KINDS = [
    "Собрать мешки (за день)", "Mansion: лимит дверей", "Победа: лимит шагов", "Hospital: зелья", "Глаза раньше мешков",
    "Победа: лимит времени, с", "Загадка с портретами", "Победа VS со случайным", "Тихий побег", "Поймать тыкву (Halloween)",
    "Покормить Good Boy", "Победа co-op со случайным", "Победа co-op с другом", "Победа VS с другом",
    "Исследовать Mansion", "Исследовать School", "Исследовать Hospital",
]

# GameFinishedReason
NETWORK_ERROR, EXIT_TO_MENU, HOST_KILLED, GUEST_KILLED, HOST_WON, GUEST_WON, CANCEL, APP_QUIT, REMOVE_ABUSER = range(9)


class Accounts:
    def __init__(self, path):
        self.db = sqlite3.connect(path)
        self.db.execute(
            "CREATE TABLE IF NOT EXISTS accounts (login TEXT PRIMARY KEY, salt BLOB, hash BLOB, created REAL)"
        )
        self.db.execute(
            "CREATE TABLE IF NOT EXISTS stats (login TEXT PRIMARY KEY, games INTEGER DEFAULT 0, won INTEGER DEFAULT 0,"
            " bags INTEGER DEFAULT 0, seconds INTEGER DEFAULT 0)"
        )
        self.db.execute("CREATE TABLE IF NOT EXISTS names (login TEXT PRIMARY KEY, name TEXT)")
        self.db.execute("CREATE TABLE IF NOT EXISTS settings (key TEXT PRIMARY KEY, value TEXT)")
        self.db.execute(
            "CREATE TABLE IF NOT EXISTS daily_custom (id INTEGER PRIMARY KEY AUTOINCREMENT, kind INTEGER, parameter INTEGER,"
            " goal INTEGER, title TEXT, description TEXT, short TEXT)"
        )
        self.db.execute("INSERT OR IGNORE INTO sqlite_sequence (name, seq) SELECT 'daily_custom', 99 WHERE NOT EXISTS (SELECT 1 FROM sqlite_sequence WHERE name='daily_custom')")
        self.db.execute(
            "CREATE TABLE IF NOT EXISTS news (id TEXT PRIMARY KEY, publish_at TEXT, title TEXT, body TEXT, category TEXT, popup INTEGER)"
        )
        for column, kind in (("exp", "INTEGER DEFAULT 0"), ("exp_override", "INTEGER DEFAULT 0"), ("distance", "INTEGER DEFAULT 0")):
            try:
                self.db.execute("ALTER TABLE stats ADD COLUMN %s %s" % (column, kind))
            except sqlite3.OperationalError:
                pass
        try:
            self.db.execute("ALTER TABLE accounts ADD COLUMN banned INTEGER DEFAULT 0")
        except sqlite3.OperationalError:
            pass
        self.db.commit()
        self.cache = {}

    @staticmethod
    def _hash(password, salt):
        return hashlib.pbkdf2_hmac("sha256", password.encode(), salt, 20000)

    def check(self, login, password):
        """Returns True for a valid credential; an unknown login becomes a new account."""
        key = (login, password)
        row = self.db.execute("SELECT salt, hash, banned FROM accounts WHERE login=?", (login,)).fetchone()
        if row is not None and row[2]:
            return False
        if key in self.cache and row is not None:
            return True
        if row is None:
            salt = secrets.token_bytes(16)
            self.db.execute(
                "INSERT INTO accounts (login, salt, hash, created) VALUES (?,?,?,?)", (login, salt, self._hash(password, salt), time.time())
            )
            self.db.commit()
            log.info("new account %s", login)
        elif not secrets.compare_digest(row[1], self._hash(password, row[0])):
            return False
        self.cache[key] = True
        return True

    def set_name(self, login, name):
        self.db.execute("INSERT OR REPLACE INTO names VALUES (?,?)", (login, name))
        self.db.commit()

    def get_name(self, login):
        row = self.db.execute("SELECT name FROM names WHERE login=?", (login,)).fetchone()
        return row[0] if row else None

    def setting(self, key, default=None):
        row = self.db.execute("SELECT value FROM settings WHERE key=?", (key,)).fetchone()
        return row[0] if row else default

    def set_setting(self, key, value):
        self.db.execute("INSERT OR REPLACE INTO settings VALUES (?,?)", (key, value))
        self.db.commit()

    def add_distance(self, login, meters):
        self.db.execute("INSERT OR IGNORE INTO stats (login) VALUES (?)", (login,))
        self.db.execute("UPDATE stats SET distance=distance+? WHERE login=?", (max(0, meters), login))
        self.db.commit()

    def sync_exp(self, login, local):
        self.db.execute("INSERT OR IGNORE INTO stats (login) VALUES (?)", (login,))
        stored, override = self.db.execute("SELECT exp, exp_override FROM stats WHERE login=?", (login,)).fetchone()
        result = stored if override else max(stored, local)
        self.db.execute("UPDATE stats SET exp=?, exp_override=0 WHERE login=?", (result, login))
        self.db.commit()
        return result

    def list_accounts(self, query):
        like = "%" + query + "%"
        rows = self.db.execute(
            "SELECT a.login, COALESCE(n.name,''), COALESCE(s.exp,0), COALESCE(s.games,0), COALESCE(s.won,0), COALESCE(s.bags,0),"
            " COALESCE(s.seconds,0), COALESCE(s.distance,0), COALESCE(a.banned,0) FROM accounts a"
            " LEFT JOIN names n ON n.login=a.login LEFT JOIN stats s ON s.login=a.login"
            " WHERE a.login LIKE ? OR COALESCE(n.name,'') LIKE ? ORDER BY a.created DESC LIMIT 200",
            (like, like),
        ).fetchall()
        keys = ("login", "name", "exp", "games", "won", "bags", "seconds", "distance", "banned")
        return [dict(zip(keys, row)) for row in rows]

    def save_account(self, data):
        login = data["login"]
        self.db.execute("INSERT OR IGNORE INTO stats (login) VALUES (?)", (login,))
        self.db.execute(
            "UPDATE stats SET exp=?, exp_override=1, games=?, won=?, bags=?, seconds=?, distance=? WHERE login=?",
            (int(data["exp"]), int(data["games"]), int(data["won"]), int(data["bags"]), int(data["seconds"]), int(data["distance"]), login),
        )
        self.db.execute("UPDATE accounts SET banned=? WHERE login=?", (1 if data.get("banned") else 0, login))
        self.set_name(login, str(data.get("name", ""))[:15])

    def delete_account(self, login):
        for table in ("accounts", "stats", "names"):
            self.db.execute("DELETE FROM %s WHERE login=?" % table, (login,))
        self.db.commit()
        self.cache = {k: v for k, v in self.cache.items() if k[0] != login}

    def list_daily_custom(self):
        rows = self.db.execute("SELECT id, kind, parameter, goal, title, description, short FROM daily_custom ORDER BY id").fetchall()
        keys = ("id", "kind", "parameter", "goal", "title", "description", "shortDescription")
        return [dict(zip(keys, row)) for row in rows]

    def save_daily_custom(self, data):
        values = (int(data["kind"]), int(data["parameter"]), max(1, int(data["goal"])), str(data["title"]), str(data.get("description", "")), str(data.get("shortDescription", "")))
        if data.get("id"):
            self.db.execute("UPDATE daily_custom SET kind=?, parameter=?, goal=?, title=?, description=?, short=? WHERE id=?", values + (int(data["id"]),))
        else:
            self.db.execute("INSERT INTO daily_custom (kind, parameter, goal, title, description, short) VALUES (?,?,?,?,?,?)", values)
        self.db.commit()

    def delete_daily_custom(self, challenge_id):
        self.db.execute("DELETE FROM daily_custom WHERE id=?", (challenge_id,))
        self.db.commit()

    def daily_plan(self):
        return json.loads(self.setting("daily_plan", "null") or "null")

    def current_daily(self):
        """The published rotation decides the challenge and its deadline."""
        plan = self.daily_plan()
        if not plan or not plan.get("ids"):
            return None
        period = max(1, int(plan["hours"])) * 3600
        step = int((time.time() - plan["start"]) // period)
        if step < 0:
            return None
        challenge_id = plan["ids"][step % len(plan["ids"])]
        deadline = plan["start"] + (step + 1) * period
        result = {"challengeId": challenge_id, "deadline": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime(deadline))}
        if challenge_id not in DAILY_BUILTIN:
            custom = [c for c in self.list_daily_custom() if c["id"] == challenge_id]
            if not custom:
                return None
            result.update({k: custom[0][k] for k in ("kind", "parameter", "goal", "title", "description", "shortDescription")})
        return result

    def list_news(self):
        rows = self.db.execute("SELECT id, publish_at, title, body, category, popup FROM news ORDER BY publish_at DESC").fetchall()
        return [{"id": r[0], "publishAt": r[1], "title": r[2], "body": r[3], "category": r[4], "showAsPopup": bool(r[5])} for r in rows]

    def save_news(self, data):
        news_id = data.get("id") or "news-" + secrets.token_hex(6)
        row = self.db.execute("SELECT publish_at FROM news WHERE id=?", (news_id,)).fetchone()
        published = row[0] if row else time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime())
        self.db.execute(
            "INSERT OR REPLACE INTO news VALUES (?,?,?,?,?,?)",
            (news_id, published, str(data["title"]), str(data["body"]), str(data.get("category", "Info")), 1 if data.get("showAsPopup") else 0),
        )
        self.db.commit()

    def delete_news(self, news_id):
        self.db.execute("DELETE FROM news WHERE id=?", (news_id,))
        self.db.commit()

    def add_game(self, login, won, bags, seconds):
        self.db.execute("INSERT OR IGNORE INTO stats (login) VALUES (?)", (login,))
        self.db.execute(
            "UPDATE stats SET games=games+1, won=won+?, bags=bags+?, seconds=seconds+? WHERE login=?",
            (1 if won else 0, bags, int(seconds), login),
        )
        self.db.commit()

    def stats(self, login):
        row = self.db.execute("SELECT games, won, bags, seconds, distance FROM stats WHERE login=?", (login,)).fetchone()
        return row or (0, 0, 0, 0, 0)


class Peer:
    def __init__(self, login, username):
        self.login = login
        self.username = username
        self.ws = None
        self.ready = False
        self.bags = 0
        self.play_again = False
        self.disconnect_task = None


class Room:
    number = 0

    def __init__(self, server, payload, token=None):
        Room.number = (Room.number % 250) + 1
        self.server = server
        self.id = secrets.token_hex(8)
        self.number = Room.number
        self.payload = payload
        self.token = token
        self.peers = [None, None]
        self.game_data = None
        self.started_at = None
        self.finished = False
        self.joined_sent = False
        self.collected = set()
        self.created = time.time()
        self.touched = time.time()

    def wire(self, player_id):
        result = {"gameId": self.id, "playerId": player_id}
        result.update(self.payload)
        return result

    @property
    def is_vs(self):
        return self.payload.get("type") == "vs"

    def other(self, player_id):
        return self.peers[1 - player_id]

    async def send(self, peer, data):
        if peer is not None and peer.ws is not None:
            await peer.ws.send(data)

    async def on_connected(self, player_id):
        if self.peers[0] is not None and self.peers[1] is not None and not self.joined_sent:
            if self.peers[0].ws is not None and self.peers[1].ws is not None:
                await self.send_joined()

    async def send_joined(self):
        self.joined_sent = True
        for i in (0, 1):
            name = self.peers[1 - i].username.encode("utf8")[:60]
            await self.send(self.peers[i], bytes([ord("J"), self.number]) + name)

    async def on_text(self, player_id, text):
        if text.startswith("R"):
            self.peers[player_id].ready = True
            if player_id == 0 and len(text) > 1:
                self.game_data = text[1:]
            await self.maybe_start()

    async def maybe_start(self):
        if self.peers[0] and self.peers[1] and self.peers[0].ready and self.peers[1].ready and self.game_data:
            self.started_at = time.time()
            self.finished = False
            self.collected = set()
            for peer in self.peers:
                peer.ready = False
                peer.bags = 0
                peer.play_again = False
                await self.send(peer, "S" + self.game_data)
            log.info("room %s started", self.id)

    async def on_binary(self, player_id, data):
        if not data:
            return
        self.touched = time.time()
        kind = data[0]
        if kind == ord("F") and len(data) >= 2:
            await self.finish(player_id, data[1])
            return
        if kind == ord("C"):
            # the score is authoritative here: the client takes the shared count from the 'R' answer
            bag = bytes(data[1:])
            await self.send(self.other(player_id), data)
            if bag not in self.collected:
                self.collected.add(bag)
                self.peers[player_id].bags += 1
                await self.send_results()
            return
        if kind == ord("B") and len(data) >= 2:
            self.peers[player_id].play_again = data[1] != 0
            await self.send(self.other(player_id), data)
            if self.peers[0] and self.peers[1] and self.peers[0].play_again and self.peers[1].play_again:
                self.peers[0].play_again = self.peers[1].play_again = False
                self.game_data = None
                await self.send_joined()
            return
        await self.send(self.other(player_id), data)

    async def send_results(self):
        results = bytes([ord("R"), self.peers[0].bags & 0xFF if self.peers[0] else 0, self.peers[1].bags & 0xFF if self.peers[1] else 0])
        await self.send(self.peers[0], results)
        await self.send(self.peers[1], results)

    def outcome(self, sender, reason):
        """Returns (host_won, guest_won) as the client's summary screen expects them."""
        if reason == HOST_WON:
            return True, not self.is_vs
        if reason == GUEST_WON:
            return not self.is_vs, True
        if reason == HOST_KILLED:
            return False, self.is_vs
        if reason == GUEST_KILLED:
            return self.is_vs, False
        if reason == REMOVE_ABUSER:
            return (sender == 0), (sender == 1)
        # the sender left: the remaining player wins a versus game and loses a co-op one
        return (self.is_vs and sender == 1), (self.is_vs and sender == 0)

    async def finish(self, sender, reason):
        if reason == CANCEL and self.peers[1 - sender] is None:
            self.server.remove_room(self)
            return
        if self.finished:
            return
        self.finished = True
        self.touched = time.time()
        wins = self.outcome(sender, reason)
        results = bytes([ord("R"), self.peers[0].bags if self.peers[0] else 0, self.peers[1].bags if self.peers[1] else 0])
        duration = time.time() - self.started_at if self.started_at else 0
        for i, peer in enumerate(self.peers):
            if peer is None:
                continue
            await self.send(peer, results)
            await self.send(peer, bytes([ord("F"), 1 if wins[i] else 0, reason]))
            if self.started_at:
                self.server.accounts.add_game(peer.login, wins[i], peer.bags, duration)
        self.started_at = None
        log.info("room %s finished sender=%d reason=%d", self.id, sender, reason)

    def on_disconnected(self, player_id):
        peer = self.peers[player_id]
        if peer is None:
            return
        peer.ws = None
        if self.finished:
            return
        if not self.joined_sent and self.peers[1 - player_id] is None:
            peer.disconnect_task = asyncio.ensure_future(self.drop_waiting(player_id))
            return
        peer.disconnect_task = asyncio.ensure_future(self.drop_after_grace(player_id))

    async def drop_waiting(self, player_id):
        await asyncio.sleep(RECONNECT_GRACE_SECONDS)
        peer = self.peers[player_id]
        if peer is not None and peer.ws is None:
            self.server.remove_room(self)

    async def drop_after_grace(self, player_id):
        await asyncio.sleep(RECONNECT_GRACE_SECONDS)
        peer = self.peers[player_id]
        if peer is not None and peer.ws is None and not self.finished:
            log.info("room %s player %d gone", self.id, player_id)
            await self.finish(player_id, EXIT_TO_MENU)


class WebSocket:
    def __init__(self, reader, writer):
        self.reader = reader
        self.writer = writer
        self.open = True
        self.lock = asyncio.Lock()

    async def send(self, data):
        if not self.open:
            return
        if isinstance(data, str):
            await self._frame(0x1, data.encode("utf8"))
        else:
            await self._frame(0x2, data)

    async def _frame(self, opcode, payload):
        head = bytearray([0x80 | opcode])
        size = len(payload)
        if size < 126:
            head.append(size)
        elif size < 65536:
            head.append(126)
            head += struct.pack(">H", size)
        else:
            head.append(127)
            head += struct.pack(">Q", size)
        try:
            async with self.lock:
                self.writer.write(bytes(head) + payload)
                await self.writer.drain()
        except (ConnectionError, OSError):
            self.open = False

    async def receive(self):
        """Returns str, bytes or None when the socket closes."""
        message = bytearray()
        message_opcode = None
        while True:
            try:
                b0, b1 = await self.reader.readexactly(2)
                size = b1 & 0x7F
                if size == 126:
                    size = struct.unpack(">H", await self.reader.readexactly(2))[0]
                elif size == 127:
                    size = struct.unpack(">Q", await self.reader.readexactly(8))[0]
                if size > MAX_WS_FRAME or len(message) + size > MAX_WS_FRAME:
                    await self.close(1009)
                    return None
                mask = await self.reader.readexactly(4) if b1 & 0x80 else None
                payload = bytearray(await self.reader.readexactly(size))
            except (asyncio.IncompleteReadError, ConnectionError, OSError):
                self.open = False
                return None
            if mask:
                for i in range(size):
                    payload[i] ^= mask[i & 3]
            opcode = b0 & 0x0F
            if opcode == 0x8:
                await self.close(1000)
                return None
            if opcode == 0x9:
                await self._frame(0xA, bytes(payload))
                continue
            if opcode == 0xA:
                continue
            if opcode in (0x1, 0x2):
                message_opcode = opcode
                message = payload
            else:
                message += payload
            if b0 & 0x80:
                if message_opcode == 0x1:
                    return bytes(message).decode("utf8", "replace")
                return bytes(message)

    async def close(self, code=1000):
        if self.open:
            await self._frame(0x8, struct.pack(">H", code))
        self.open = False
        try:
            self.writer.close()
        except OSError:
            pass


class Server:
    def __init__(self, accounts, levels, admin_password):
        self.accounts = accounts
        self.levels = levels
        self.admin_password = admin_password
        with open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "admin.html"), "rb") as source:
            self.admin_page = source.read()
        self.rooms = {}
        self.tokens = {}
        self.queue = {}

    # ---------- HTTP ----------

    async def handle(self, reader, writer):
        try:
            while True:
                request = await self.read_request(reader)
                if request is None:
                    break
                method, target, headers, body = request
                parsed = urllib.parse.urlsplit(target)
                if headers.get("upgrade", "").lower() == "websocket":
                    await self.handle_websocket(parsed.path, headers, reader, writer)
                    return
                if parsed.path == "/admin" or parsed.path.startswith("/admin/"):
                    await self.handle_admin(method, parsed, headers, body, writer)
                    continue
                status, payload = self.route(method, parsed.path, urllib.parse.parse_qs(parsed.query), headers, body)
                data = json.dumps(payload).encode()
                writer.write(
                    b"HTTP/1.1 %d %s\r\nContent-Type: application/json\r\nContent-Length: %d\r\n\r\n"
                    % (status, b"OK" if status == 200 else b"Error", len(data))
                    + data
                )
                await writer.drain()
        except (ConnectionError, OSError, asyncio.IncompleteReadError, ValueError, asyncio.TimeoutError):
            pass
        finally:
            try:
                writer.close()
            except OSError:
                pass

    # ---------- admin panel ----------

    def admin_authorized(self, headers):
        value = headers.get("authorization", "")
        if not value.startswith("Basic "):
            return False
        try:
            user, _, password = base64.b64decode(value[6:]).decode("utf8").partition(":")
        except ValueError:
            return False
        return user == "admin" and secrets.compare_digest(password.encode(), self.admin_password.encode())

    async def handle_admin(self, method, parsed, headers, body, writer):
        extra = b""
        if not self.admin_authorized(headers):
            await asyncio.sleep(1)
            status, content_type, data = 401, b"text/plain", b"Unauthorized"
            extra = b'WWW-Authenticate: Basic realm="Eyes admin", charset="UTF-8"\r\n'
        elif parsed.path == "/admin":
            status, content_type, data = 200, b"text/html; charset=utf-8", self.admin_page
        elif headers.get("x-admin") != "1":
            # browsers cannot add this header to a cross-site form post
            status, content_type, data = 403, b"application/json", b'{"reason":"forbidden"}'
        else:
            try:
                code, payload = await self.admin_api(method, parsed.path[len("/admin/api/"):], urllib.parse.parse_qs(parsed.query), body)
            except (ValueError, KeyError, TypeError, IndexError, AttributeError) as error:
                code, payload = 400, {"reason": "invalid request: %s" % error}
            status, content_type, data = code, b"application/json", json.dumps(payload).encode()
        writer.write(
            b"HTTP/1.1 %d %s\r\nContent-Type: %s\r\nContent-Length: %d\r\nCache-Control: no-store\r\nX-Frame-Options: DENY\r\n%s\r\n"
            % (status, b"OK" if status == 200 else b"Error", content_type, len(data), extra)
            + data
        )
        await writer.drain()

    def room_view(self, room):
        def player(peer):
            if peer is None:
                return None
            return {"login": peer.login, "username": peer.username, "connected": peer.ws is not None, "bags": peer.bags, "ready": peer.ready}

        return {
            "id": room.id,
            "number": room.number,
            "token": room.token,
            "payload": room.payload,
            "players": [player(p) for p in room.peers],
            "started": room.started_at is not None,
            "duration": int(time.time() - room.started_at) if room.started_at else 0,
            "finished": room.finished,
            "game_data": room.game_data,
        }

    async def admin_api(self, method, path, query, body):
        data = json.loads(body.decode("utf8")) if method == "POST" and body else {}
        if path == "rooms":
            rooms = [self.room_view(room) for room in self.rooms.values()]
            online = sum(1 for room in self.rooms.values() for peer in room.peers if peer is not None and peer.ws is not None)
            return 200, {"rooms": rooms, "online": online}
        if path.startswith("rooms/"):
            room = self.rooms.get(data["room"])
            if room is None:
                return 404, {"reason": "room not found"}
            action = path[len("rooms/"):]
            if action == "bags":
                peer = room.peers[int(data["player"])]
                peer.bags = max(0, min(255, int(data["bags"])))
                await room.send_results()
            elif action == "kick":
                peer = room.peers[int(data["player"])]
                if peer.ws is not None:
                    await peer.ws.close(4002)
            elif action == "finish":
                await room.finish(0, int(data["reason"]))
            elif action == "close":
                for peer in room.peers:
                    if peer is not None and peer.ws is not None:
                        await peer.ws.close(4003)
                self.remove_room(room)
            elif action == "gamedata":
                room.game_data = str(data["game_data"]) or None
                await room.maybe_start()
            else:
                return 404, {"reason": "unknown action"}
            log.info("admin %s room %s", action, room.id)
            return 200, {"ok": True}
        if path == "accounts":
            return 200, {"accounts": self.accounts.list_accounts(query.get("q", [""])[0])}
        if path == "accounts/save":
            self.accounts.save_account(data)
            return 200, {"ok": True}
        if path == "accounts/delete":
            self.accounts.delete_account(str(data["login"]))
            return 200, {"ok": True}
        if path == "news":
            return 200, {"items": self.accounts.list_news()}
        if path == "news/save":
            self.accounts.save_news(data)
            return 200, {"ok": True}
        if path == "news/delete":
            self.accounts.delete_news(str(data["id"]))
            return 200, {"ok": True}
        if path == "daily":
            builtin = [{"id": k, "kind": v[0], "parameter": v[1], "goal": v[2], "title": v[3], "builtin": True} for k, v in DAILY_BUILTIN.items()]
            return 200, {"challenges": builtin + self.accounts.list_daily_custom(), "kinds": DAILY_KINDS, "plan": self.accounts.daily_plan(), "current": self.accounts.current_daily()}
        if path == "daily/save":
            self.accounts.save_daily_custom(data)
            return 200, {"ok": True}
        if path == "daily/delete":
            self.accounts.delete_daily_custom(int(data["id"]))
            return 200, {"ok": True}
        if path == "daily/publish":
            ids = [int(i) for i in data["ids"]]
            if not ids:
                return 400, {"reason": "выберите хотя бы одно испытание"}
            self.accounts.set_setting("daily_plan", json.dumps({"ids": ids, "hours": max(1, int(data["hours"])), "start": time.time()}))
            return 200, {"ok": True, "current": self.accounts.current_daily()}
        if path == "daily/stop":
            self.accounts.set_setting("daily_plan", "null")
            return 200, {"ok": True}
        if path == "settings":
            if method == "POST":
                self.accounts.set_setting("test_mode", "1" if data.get("test_mode") else "0")
            return 200, {"test_mode": self.test_mode}
        return 404, {"reason": "not found"}

    @property
    def test_mode(self):
        return self.accounts.setting("test_mode", "1") == "1"

    @staticmethod
    async def read_request(reader):
        line = await reader.readline()
        if not line:
            return None
        parts = line.decode("latin1").split()
        if len(parts) != 3:
            raise ValueError("bad request line")
        headers = {}
        for _ in range(100):
            line = await reader.readline()
            if line in (b"\r\n", b"\n", b""):
                break
            name, _, value = line.decode("latin1").partition(":")
            headers[name.strip().lower()] = value.strip()
        size = int(headers.get("content-length", "0") or 0)
        if size > MAX_HTTP_BODY:
            raise ValueError("body too large")
        body = await reader.readexactly(size) if size else b""
        return parts[0].upper(), parts[1], headers, body

    def authenticate(self, headers):
        value = headers.get("authorization", "")
        if not value.startswith("Basic "):
            return None
        try:
            login, _, password = base64.b64decode(value[6:]).decode("utf8").partition(":")
        except ValueError:
            return None
        if not login or not password or not self.accounts.check(login, password):
            return None
        return login

    @staticmethod
    def username(headers, login):
        name = headers.get("x-game-user", "").strip()
        return name[:30] if name and not name.startswith("<<") else "Player" + login[-4:]

    def route(self, method, path, query, headers, body):
        if path == "/api/v1/ping":
            return 200, {"ok": True, "free_keys": self.test_mode}
        if path == "/api/v1/news":
            return 200, {"items": self.accounts.list_news()}
        if path.endswith("/challenges/daily/current"):
            current = self.accounts.current_daily()
            return (200, current) if current else (404, {"code": "no-challenge", "reason": "no active challenge"})
        if path.startswith("/api/v1/") and path.split("/")[3] in ("find", "create", "login"):
            try:
                if int(headers.get("client-version", "0")) < MIN_CLIENT_VERSION:
                    return 400, {"code": "old-version", "reason": "please update the game"}
            except ValueError:
                return 400, {"code": "old-version", "reason": "please update the game"}
            login = self.authenticate(headers)
            if login is None:
                return 401, {"code": "unauthorized", "reason": "invalid credentials"}
            if path == "/api/v1/login":
                return 200, {"username": login}
            if path == "/api/v1/create" and method == "POST":
                return self.create(login, headers, body)
            if path == "/api/v1/find" and method == "POST":
                return self.find(login, headers, body)
            if path == "/api/v1/find":
                return self.find_by_token(login, headers, query.get("token", [""])[0])
            return 404, {"code": "not-found", "reason": path}
        login = self.authenticate(headers)
        if path.endswith("/games/stats/bag-collection"):
            return 200, self.bag_stats(login)
        if path.endswith("/user/name") and login:
            if method == "POST":
                try:
                    name = str(json.loads(body.decode("utf8") or "{}").get("name", "")).strip()
                except ValueError:
                    return 422, {"code": "invalid-payload", "reason": "invalid request"}
                if not 3 <= len(name) <= 15:
                    return 422, {"code": "invalid-name", "reason": "Username must be between 3 and 15 characters"}
                self.accounts.set_name(login, name)
                return 200, {"name": name}
            return 200, {"name": self.accounts.get_name(login) or ""}
        if path.endswith("/user/sync-info") and login:
            try:
                data = json.loads(body.decode("utf8") or "{}")
            except ValueError:
                data = {}
            stored = self.accounts.get_name(login)
            if not stored and data.get("username"):
                self.accounts.set_name(login, str(data["username"])[:15])
                stored = self.accounts.get_name(login)
            distance = int(data.get("localDistance", 0) or 0)
            exp = int(data.get("localExp", 0) or 0)
            return 200, {"previousLocalExp": exp, "newLocalExp": self.accounts.sync_exp(login, exp), "username": stored or "", "previousLocalDistance": distance, "newLocalDistance": distance}
        if path.endswith("/user/stats/game-distance") and login:
            try:
                self.accounts.add_distance(login, int(json.loads(body.decode("utf8") or "{}").get("distance", 0)))
            except (ValueError, TypeError):
                return 422, {"code": "invalid-payload", "reason": "invalid request"}
            return 200, {}
        return 200, {}

    def bag_stats(self, login):
        games, won, bags, seconds, distance = self.accounts.stats(login) if login else (0, 0, 0, 0, 0)
        minutes = seconds / 60 if seconds else 0
        total = {
            "won_games": won,
            "total_games": games,
            "won_bag_events": 0,
            "total_bag_events": bags,
            "won_duration_seconds": 0,
            "total_duration_seconds": seconds,
            "won_events_per_minute": 0.0,
            "events_per_minute": bags / minutes if minutes else 0.0,
            "distance": distance,
        }
        return {"all_time": total, "current_week": total, "last_week": total}

    def parse_payload(self, body):
        try:
            payload = json.loads(body.decode("utf8") or "{}")
        except ValueError:
            return None, (422, {"code": "invalid-payload", "reason": "invalid request"})
        keep = {k: str(payload.get(k, "")) for k in ("type", "difficulty", "mode", "enemy", "level")}
        if self.levels and keep["level"].lower() not in self.levels:
            return None, (422, {"code": "unsupported-level", "reason": keep["level"]})
        return keep, None

    def new_token(self):
        while True:
            token = "".join(secrets.choice(TOKEN_ALPHABET) for _ in range(5))
            if token not in self.tokens:
                return token

    def create(self, login, headers, body):
        payload, error = self.parse_payload(body)
        if error:
            return error
        room = Room(self, payload, self.new_token())
        room.peers[0] = Peer(login, self.username(headers, login))
        self.rooms[room.id] = room
        self.tokens[room.token] = room
        result = room.wire(0)
        result["token"] = room.token
        log.info("room %s created by %s token %s", room.id, login, room.token)
        return 200, result

    def find(self, login, headers, body):
        payload, error = self.parse_payload(body)
        if error:
            return error
        key = json.dumps(payload, sort_keys=True)
        waiting = self.queue.get(key, [])
        while waiting:
            room = waiting.pop(0)
            host = room.peers[0]
            if room.id in self.rooms and room.peers[1] is None and host is not None and host.ws is not None and host.login != login:
                room.peers[1] = Peer(login, self.username(headers, login))
                log.info("room %s matched %s", room.id, login)
                return 200, room.wire(1)
        room = Room(self, payload)
        room.peers[0] = Peer(login, self.username(headers, login))
        self.rooms[room.id] = room
        self.queue.setdefault(key, []).append(room)
        log.info("room %s waiting, host %s", room.id, login)
        return 200, room.wire(0)

    def find_by_token(self, login, headers, token):
        token = token.strip().upper()
        if len(token) != 5:
            return 404, {"code": "invalid-token", "reason": "Invalid invitation code"}
        room = self.tokens.get(token)
        if room is None or room.id not in self.rooms or room.peers[1] is not None:
            return 404, {"code": "token-not-found", "reason": "Invitation code not found"}
        locked = self.locked_content(room.payload, headers)
        if locked:
            return 422, {"code": "content-locked", "reason": locked}
        room.peers[1] = Peer(login, self.username(headers, login))
        return 200, room.wire(1)

    @staticmethod
    def locked_content(payload, headers):
        for header, field, free in (("se", "enemy", {"krasue", "goose"}), ("sl", "level", {"mansion"})):
            allowed = headers.get(header)
            value = payload.get(field, "").lower()
            if allowed and value not in free:
                if value not in [item.strip(' "').lower() for item in allowed.split(",")]:
                    return payload.get(field)
        return None

    def remove_room(self, room):
        self.rooms.pop(room.id, None)
        if room.token:
            self.tokens.pop(room.token, None)
        log.info("room %s removed", room.id)

    # ---------- WebSocket ----------

    @staticmethod
    async def accept_websocket(key, reader, writer):
        accept = base64.b64encode(hashlib.sha1((key + WS_GUID).encode()).digest())
        writer.write(
            b"HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: "
            + accept
            + b"\r\n\r\n"
        )
        await writer.drain()
        return WebSocket(reader, writer)

    async def handle_diag(self, headers, reader, writer):
        ws = await self.accept_websocket(headers.get("sec-websocket-key", ""), reader, writer)
        while True:
            message = await asyncio.wait_for(ws.receive(), 30)
            if message is None:
                return
            await ws.send(message)

    async def handle_websocket(self, path, headers, reader, writer):
        if path == "/ws/v1/diag":
            await self.handle_diag(headers, reader, writer)
            return
        parts = path.strip("/").split("/")
        room = None
        player_id = -1
        if len(parts) == 5 and parts[:2] == ["ws", "v1"] and parts[3] == "join":
            room = self.rooms.get(parts[2])
            player_id = int(parts[4]) if parts[4] in ("0", "1") else -1
        login = self.authenticate(headers)
        key = headers.get("sec-websocket-key", "")
        peer = room.peers[player_id] if room is not None and player_id >= 0 else None
        if login is None or peer is None or peer.login != login or not key:
            writer.write(b"HTTP/1.1 %s\r\nContent-Length: 0\r\n\r\n" % (b"401 Unauthorized" if login is None else b"404 Not Found"))
            await writer.drain()
            writer.close()
            return
        ws = await self.accept_websocket(key, reader, writer)
        if peer.ws is not None:
            await peer.ws.close(4001)
        if peer.disconnect_task is not None:
            peer.disconnect_task.cancel()
            peer.disconnect_task = None
        peer.ws = ws
        room.touched = time.time()
        log.info("room %s player %d connected", room.id, player_id)
        await room.on_connected(player_id)
        while True:
            message = await ws.receive()
            if message is None:
                break
            if isinstance(message, str):
                await room.on_text(player_id, message)
            else:
                await room.on_binary(player_id, message)
        if peer.ws is ws:
            room.on_disconnected(player_id)
        log.info("room %s player %d disconnected", room.id, player_id)

    async def janitor(self):
        while True:
            await asyncio.sleep(30)
            now = time.time()
            for room in list(self.rooms.values()):
                connected = any(p is not None and p.ws is not None for p in room.peers)
                ttl = FINISHED_ROOM_TTL_SECONDS if room.finished else WAITING_ROOM_TTL_SECONDS
                if not connected and now - room.touched > ttl:
                    self.remove_room(room)
            for key in list(self.queue):
                self.queue[key] = [r for r in self.queue[key] if r.id in self.rooms and r.peers[1] is None]
                if not self.queue[key]:
                    del self.queue[key]


async def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--host", default="0.0.0.0")
    parser.add_argument("--port", type=int, default=int(os.environ.get("PORT", "8080")))
    parser.add_argument("--data", default="eyes_server.db")
    parser.add_argument("--levels", default="", help="comma separated list of playable levels, empty for all")
    parser.add_argument("--admin-password", default=os.environ.get("EYES_ADMIN_PASSWORD"))
    parser.add_argument("--cert")
    parser.add_argument("--key")
    args = parser.parse_args()
    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
    levels = {item.strip().lower() for item in args.levels.split(",") if item.strip()}
    accounts = Accounts(args.data)
    admin_password = args.admin_password or accounts.setting("admin_password")
    if not admin_password:
        admin_password = secrets.token_urlsafe(12)
        accounts.set_setting("admin_password", admin_password)
        log.info("generated admin password: %s (user: admin)", admin_password)
    server = Server(accounts, levels, admin_password)
    context = None
    if args.cert:
        context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
        context.load_cert_chain(args.cert, args.key)
    listener = await asyncio.start_server(server.handle, args.host, args.port, ssl=context)
    log.info("listening on %s:%d (tls=%s) levels=%s", args.host, args.port, bool(context), sorted(levels) or "all")
    asyncio.ensure_future(server.janitor())
    async with listener:
        await listener.serve_forever()


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        pass
