#!/usr/bin/env python3
"""End-to-end protocol check: two fake clients through a running eyes_server.py."""
import asyncio
import base64
import json
import os
import struct
import sys
import urllib.error
import urllib.request

BASE = sys.argv[1] if len(sys.argv) > 1 else "127.0.0.1:18080"


def auth(name):
    return "Basic " + base64.b64encode(("%s:secret-%s" % (name, name)).encode()).decode()


def call(path, name, body=None, method=None, extra=None):
    headers = {"Authorization": auth(name), "Client-Version": "4", "X-Game-User": name}
    headers.update(extra or {})
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request("http://%s%s" % (BASE, path), data=data, headers=headers, method=method or ("POST" if data else "GET"))
    try:
        with urllib.request.urlopen(req) as response:
            return response.status, json.loads(response.read())
    except urllib.error.HTTPError as error:
        return error.code, json.loads(error.read())


def admin(path, body=None):
    headers = {"Authorization": "Basic " + base64.b64encode(b"admin:test-admin").decode(), "X-Admin": "1"}
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request("http://%s/admin/api/%s" % (BASE, path), data=data, headers=headers)
    try:
        with urllib.request.urlopen(req) as response:
            return response.status, json.loads(response.read())
    except urllib.error.HTTPError as error:
        return error.code, json.loads(error.read())


class Ws:
    async def connect(self, game, player, name):
        host, port = BASE.split(":")
        self.r, self.w = await asyncio.open_connection(host, int(port))
        key = base64.b64encode(os.urandom(16)).decode()
        self.w.write(("GET /ws/v1/%s/join/%d HTTP/1.1\r\nHost: x\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Key: %s\r\nSec-WebSocket-Version: 13\r\nAuthorization: %s\r\n\r\n" % (game, player, key, auth(name))).encode())
        await self.w.drain()
        head = await self.r.readuntil(b"\r\n\r\n")
        assert head.startswith(b"HTTP/1.1 101"), head
        return self

    async def send(self, data):
        op = 1 if isinstance(data, str) else 2
        data = data.encode() if isinstance(data, str) else data
        mask = os.urandom(4)
        head = bytes([0x80 | op])
        head += bytes([0x80 | len(data)]) if len(data) < 126 else bytes([0x80 | 126]) + struct.pack(">H", len(data))
        self.w.write(head + mask + bytes(c ^ mask[i & 3] for i, c in enumerate(data)))
        await self.w.drain()

    async def recv(self):
        b0, b1 = await asyncio.wait_for(self.r.readexactly(2), 5)
        size = b1
        if size == 126:
            size = struct.unpack(">H", await self.r.readexactly(2))[0]
        data = await self.r.readexactly(size)
        return data.decode() if b0 & 0xF == 1 else data


async def scenario():
    assert call("/api/v1/ping", "a")[0] == 200
    assert call("/api/v1/login", "a", {}, "POST")[0] == 200
    bad = urllib.request.Request("http://%s/api/v1/login" % BASE, data=b"{}", headers={"Authorization": "Basic " + base64.b64encode(b"a:wrong").decode(), "Client-Version": "4"}, method="POST")
    try:
        urllib.request.urlopen(bad)
        raise AssertionError("wrong password accepted")
    except urllib.error.HTTPError as error:
        assert error.code == 401
    payload = {"type": "coop", "difficulty": "normal", "mode": "classic", "enemy": "krasue", "level": "mansion"}
    assert call("/api/v1/find", "a", dict(payload, level="hospital"))[0] == 422
    status, first = call("/api/v1/find", "a", payload)
    assert status == 200 and first["playerId"] == 0, first
    host = await Ws().connect(first["gameId"], 0, "a")
    status, second = call("/api/v1/find", "b", payload)
    assert status == 200 and second["gameId"] == first["gameId"] and second["playerId"] == 1, second
    guest = await Ws().connect(second["gameId"], 1, "b")
    joined = await host.recv()
    assert joined[0] == ord("J") and joined[2:] == b"b", joined
    joined = await guest.recv()
    assert joined[0] == ord("J") and joined[2:] == b"a", joined
    await guest.send("R")
    await host.send('R{"saw":"x"}')
    assert await host.recv() == 'S{"saw":"x"}'
    assert await guest.recv() == 'S{"saw":"x"}'
    move = b"M" + bytes(32)
    await host.send(move)
    assert await guest.recv() == move
    await guest.send(b"C" + b"bag")
    assert await host.recv() == b"C" + b"bag"
    assert await host.recv() == bytes([ord("R"), 0, 1])
    assert await guest.recv() == bytes([ord("R"), 0, 1])
    # the same bag twice does not count
    await host.send(b"C" + b"bag")
    assert await guest.recv() == b"C" + b"bag"
    await host.send(bytes([ord("F"), 4]))
    assert (await host.recv()) == bytes([ord("R"), 0, 1])
    assert (await host.recv()) == bytes([ord("F"), 1, 4])
    assert (await guest.recv()) == bytes([ord("R"), 0, 1])
    assert (await guest.recv()) == bytes([ord("F"), 1, 4])
    # play again
    await host.send(bytes([ord("B"), 1]))
    assert await guest.recv() == bytes([ord("B"), 1])
    await guest.send(bytes([ord("B"), 1]))
    assert await host.recv() == bytes([ord("B"), 1])
    assert (await host.recv())[0] == ord("J")
    assert (await guest.recv())[0] == ord("J")
    # friend game
    status, created = call("/api/v1/create", "a", dict(payload, type="vs"))
    assert status == 200 and len(created["token"]) == 5, created
    h2 = await Ws().connect(created["gameId"], 0, "a")
    assert call("/api/v1/find?token=ZZZZZ", "b")[0] == 404
    status, joined_game = call("/api/v1/find?token=" + created["token"], "b", extra={"se": '"krasue"', "sl": '"mansion"', "sm": '"classic"'})
    assert status == 200 and joined_game["playerId"] == 1, joined_game
    g2 = await Ws().connect(joined_game["gameId"], 1, "b")
    await h2.recv()
    await g2.recv()
    # versus: host leaves, guest wins
    await h2.send(bytes([ord("F"), 1]))
    await g2.recv()
    assert (await g2.recv()) == bytes([ord("F"), 1, 1])
    # news, free keys flag, admin panel
    assert call("/api/v1/ping", "a")[1]["free_keys"] is True
    assert admin("settings", {"test_mode": False})[1]["test_mode"] is False
    assert call("/api/v1/ping", "a")[1]["free_keys"] is False
    admin("settings", {"test_mode": True})
    assert admin("news/save", {"title": "T", "body": "B", "category": "Update", "showAsPopup": True})[0] == 200
    item = call("/api/v1/news", "a")[1]["items"][0]
    assert item["title"] == "T" and item["showAsPopup"] is True and item["id"]
    accounts = admin("accounts")[1]["accounts"]
    assert {x["login"] for x in accounts} >= {"a", "b"}
    target = [x for x in accounts if x["login"] == "b"][0]
    target.update(exp=500, banned=True, name="Edited")
    assert admin("accounts/save", target)[0] == 200
    bad = call("/api/v1/login", "b", {}, "POST")
    assert bad[0] == 401, bad
    target["banned"] = False
    admin("accounts/save", target)
    assert call("/api/v1/user/name", "b")[1]["name"] == "Edited"
    assert call("/api/v1/user/sync-info", "b", {"localExp": 10})[1]["newLocalExp"] == 500
    assert call("/api/v1/challenges/daily/current", "a")[0] == 404
    assert admin("daily/save", {"kind": 2, "parameter": 300, "goal": 1, "title": "Custom", "description": "D", "shortDescription": "S"})[0] == 200
    daily = admin("daily")[1]
    custom = [c for c in daily["challenges"] if not c.get("builtin")][0]
    assert custom["id"] >= 100, custom
    assert admin("daily/publish", {"ids": [custom["id"]], "hours": 24})[0] == 200
    current = call("/api/v1/challenges/daily/current", "a")[1]
    assert current["challengeId"] == custom["id"] and current["title"] == "Custom" and current["parameter"] == 300, current
    assert admin("daily/publish", {"ids": [3], "hours": 24})[0] == 200
    current = call("/api/v1/challenges/daily/current", "a")[1]
    assert current["challengeId"] == 3 and "title" not in current and current["deadline"].endswith("Z"), current
    rooms = admin("rooms")[1]["rooms"]
    assert rooms, rooms
    assert admin("rooms/bags", {"room": rooms[0]["id"], "player": 0, "bags": 7})[0] == 200
    try:
        urllib.request.urlopen(urllib.request.Request("http://%s/admin/api/rooms" % BASE))
        raise AssertionError("admin open without password")
    except urllib.error.HTTPError as error:
        assert error.code == 401
    print("PROTOCOL OK")


asyncio.run(scenario())
