# OpenEthg_RE server

Requires Python 3.8+ and nothing else.

    python eyes_server.py --port 8080 --data eyes_server.db

Options: `--levels mansion` limits playable maps (empty = all), `--cert/--key` enable TLS (wss/https).
Accounts, names and stats are in the SQLite file given by `--data`; back it up.

Client address, first match wins: `EYES_SERVER` env, `server.txt` beside the exe (PC),
`Assets/Resources/online_server.txt` (needs a rebuild, this is the only way on Android).
Format: `http://host:port` or `https://host`.

Check: `python test_protocol.py 127.0.0.1:8080` against a running server.

## Quick start on Windows

`start_server.bat` checks for Python, keeps data in `data\` next to it and opens the panel. The alpha build
copies these files to `eyes_alpha_pc\Server`. Do not include that folder in a build given to players.

## Admin panel

Open `http://host:port/admin`, user `admin`. Password: `--admin-password` or `EYES_ADMIN_PASSWORD`;
without either a random one is generated on first start, printed to the log and kept in the database.
HTTP Basic sends the password with every request, so on a public address run the server with `--cert/--key`.

Panel: live rooms (bags, kick, finish, close, start data), accounts (name, exp, stats, ban, delete),
news (shown to players on a custom server, read state is local), free keys switch (custom server only).
