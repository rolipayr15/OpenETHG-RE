using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bibloteka;
using BestHTTP;
using BestHTTP.WebSocket;
using EyesAnalytics;
using I2.Loc;
using Multiplayer;
using Multiplayer.ExternalApi;
using Prime31.MessageKit;
using Unity.Services.Authentication;
using UnityEngine;

public class MultiplayerSettings : CustomSingleton<MultiplayerSettings>
{
	private static WebSocket websocket;

	public static MultiGame game;

	private static string requestIdHeaderName = "req";

	private int retryFailures;

	private Coroutine coroutine;

	private static readonly ReviewerBasicAuthSession reviewerBasicAuthSession = new ReviewerBasicAuthSession();

	private bool isConnected;

	private WebSocket gameSocket;

	private GameFinishedReason reasonToSentWhenReconnected;

	private void Start()
	{
		MakeDontDestroyOnLoad();
	}

	private string BuildWebSocketUrl(string gameId, int playerId)
	{
		return ApiConfiguration.GetBackendWebSocketUrl(gameId, playerId);
	}

	public void JoinOrCreate(GameMode gameType, Difficulty difficulty, Mode mode, Enemy enemy, Level level)
	{
		Debug.Log("Join or create a game. Game type " + gameType.ToString() + " with difficulty " + difficulty.ToString());
		PostGame("/api/v1/find", gameType, difficulty, mode, enemy, level, ParseGame);
	}

	private void PostGame(string path, GameMode gameType, Difficulty difficulty, Mode mode, Enemy enemy, Level level, Action<HTTPResponse> parse)
	{
		string url = ApiConfiguration.GetBackendHttpUrl(path);
		Debug.Log("Requesting " + url);
		HTTPRequest request = new HTTPRequest(new Uri(url), HTTPMethods.Post, delegate(HTTPRequest req, HTTPResponse response)
		{
			ParseResponse(req, response, parse);
		});
		AuthorizeRequest(request);
		request.AddHeader("Content-Type", "application/json");
		request.RawData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(BuildPayload(gameType, difficulty, mode, enemy, level)));
		request.Send();
	}

	private void ParseGame(HTTPResponse response)
	{
		game = JsonUtility.FromJson<MultiGame>(response.DataAsText);
		Debug.Log("Found game id = " + game.gameId + ", player id = " + game.playerId + ", type = " + game.type + ", difficulty = " + game.difficulty + ", enemy = " + game.enemy);
		UserGameSettings.isMultiplayerHost = game.playerId == 0;
		MessageKit<MultiGame>.post(83, game);
	}

	private void ParseGameWithToken(HTTPResponse response)
	{
		GameWithToken withToken = JsonUtility.FromJson<GameWithToken>(response.DataAsText);
		game = new MultiGame(withToken.gameId, withToken.playerId, withToken.type, withToken.difficulty, withToken.mode, withToken.enemy, withToken.level);
		Debug.Log("Found game id = " + withToken.gameId + ", player id = " + withToken.playerId + ", type = " + withToken.type + ", difficulty = " + withToken.difficulty + ", enemy = " + game.enemy + ", token = " + withToken.token);
		UserGameSettings.isMultiplayerHost = game.playerId == 0;
		MessageKit<GameWithToken>.post(84, withToken);
	}

	private void ParseResponse(HTTPRequest request, HTTPResponse response, Action<HTTPResponse> action)
	{
		string message;
		if (response == null)
		{
			Debug.LogError("Request failed " + request.Exception);
			message = "cannot connect to server. " + getRequestId(request);
		}
		else
		{
			if (!response.IsSuccess)
			{
				MultiplayerServer.ResponseErrorEvent(response.StatusCode);
			}
			if (response.IsSuccess)
			{
				action?.Invoke(response);
				return;
			}
			switch (response.StatusCode)
			{
			case 400:
				Debug.Log("Request " + request.CurrentUri + " failed. User uses old version.");
				message = "please update the game";
				break;
			case 422:
			{
				ErrorResponse unsupported = JsonUtility.FromJson<ErrorResponse>(response.DataAsText);
				string text = unsupported.reason;
				if (unsupported.code == "unsupported-enemy" || unsupported.code == "unsupported-level" || unsupported.code == "unsupported-mode")
				{
					text = LocalizationManager.GetTranslation("GooseUpdateGame");
				}
				MessageKit<string>.post(111, text);
				return;
			}
			case 401:
				Debug.LogError("Request " + request.CurrentUri + " failed. User is unauthorized.");
				message = "User is unauthorized";
				break;
			default:
			{
				ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(response.DataAsText);
				MessageKit<ErrorResponse>.post(88, error);
				message = error.code + ": " + error.reason;
				break;
			}
			}
		}
		MessageKit<string>.post(87, message);
	}

	private string getRequestId(HTTPRequest req)
	{
		return "Id:" + req.GetFirstHeaderValue(requestIdHeaderName);
	}

	public void CreateGameWithToken(GameMode gameType, Difficulty difficulty, Mode mode, Enemy enemy, Level level)
	{
		Debug.Log("Create a game. Game type " + gameType.ToString() + " with difficulty " + difficulty.ToString());
		PostGame("/api/v1/create", gameType, difficulty, mode, enemy, level, ParseGameWithToken);
	}

	private static GamePayload BuildPayload(GameMode gameType, Difficulty difficulty, Mode mode, Enemy enemy, Level level)
	{
		(string enemyWire, string modeWire) = MultiplayerWireSettings.ToWire(enemy, mode);
		return new GamePayload(MultiplayerWireSettings.ToWire(gameType), MultiplayerWireSettings.ToWire(difficulty), modeWire, enemyWire, MultiplayerWireSettings.ToWire(level));
	}

	private static string QuotedList(params object[] values)
	{
		return string.Join(",", values.Select((object value) => "\"" + value.ToString().ToLower() + "\""));
	}

	private void AddUnlockedContentHeaders(HTTPRequest request)
	{
		if (UserGameSettings.isInvitedPlayer)
		{
			request.AddHeader("se", QuotedList((Enemy)0, (Enemy)1, (Enemy)2, (Enemy)4, (Enemy)5));
			request.AddHeader("sl", QuotedList((Level)0, (Level)1, (Level)3));
			request.AddHeader("sm", QuotedList((Mode)0, (Mode)1, (Mode)2, (Mode)3));
			return;
		}
		StringBuilder enemies = new StringBuilder(QuotedList((Enemy)0));
		if (RecoveryTestBuild.Unlocked(UserGameSettings.unlockedCharlie))
		{
			enemies.Append(",").Append(QuotedList((Enemy)1));
		}
		if (RecoveryTestBuild.Unlocked(UserGameSettings.unlockedDog))
		{
			enemies.Append(",").Append(QuotedList((Enemy)2));
		}
		enemies.Append(",").Append(QuotedList((Enemy)4));
		if (RecoveryTestBuild.Unlocked(UserGameSettings.unlockedDoubleEnemyMode))
		{
			enemies.Append(",").Append(QuotedList((Enemy)5));
		}
		if (RecoveryTestBuild.Unlocked(UserGameSettings.unlockedWitch))
		{
			enemies.Append(",").Append(QuotedList((Enemy)6));
		}
		request.AddHeader("se", enemies.ToString());
		StringBuilder levels = new StringBuilder(QuotedList((Level)0));
		if (RecoveryTestBuild.LevelUnlocked(Level.Hospital))
		{
			levels.Append(",").Append(QuotedList((Level)1));
		}
		if (RecoveryTestBuild.LevelUnlocked(Level.School))
		{
			levels.Append(",").Append(QuotedList((Level)3));
		}
		request.AddHeader("sl", levels.ToString());
		StringBuilder modes = new StringBuilder(QuotedList((Mode)0, (Mode)2));
		if (RecoveryTestBuild.Unlocked(UserGameSettings.unlockedEndlessMode))
		{
			modes.Append(",").Append(QuotedList((Mode)1));
		}
		modes.Append(",").Append(QuotedList((Mode)3));
		request.AddHeader("sm", modes.ToString());
	}

	public void FindByToken(string token)
	{
		string url = ApiConfiguration.GetBackendHttpUrl("/api/v1/find") + "?token=" + token;
		Debug.Log("Search a game by token " + token);
		Debug.Log("Requesting " + url);
		HTTPRequest request = new HTTPRequest(new Uri(url), delegate(HTTPRequest req, HTTPResponse response)
		{
			ParseResponse(req, response, ParseGame);
		});
		AuthorizeRequest(request);
		AddUnlockedContentHeaders(request);
		Debug.Log(request.GetFirstHeaderValue("sm"));
		Debug.Log(request.GetFirstHeaderValue("sl"));
		Debug.Log(request.GetFirstHeaderValue("se"));
		request.Send();
	}

	public async Task GetStats()
	{
		AllStats stats = await new StatsApi().GetStatsAsync();
		if (stats != null)
		{
			UserGameSettings.stats = stats;
		}
	}

	public static string GetReviewerBasicAuthCredential()
	{
		return string.IsNullOrEmpty(reviewerBasicAuthSession.Credential) ? null : reviewerBasicAuthSession.Credential;
	}

	public HTTPRequest SendReviewerBasicAuthLogin(string candidateCredential)
	{
		if (string.IsNullOrEmpty(candidateCredential))
		{
			throw new ArgumentException("A reviewer Basic credential is required.", "candidateCredential");
		}
		HTTPRequest request = new HTTPRequest(new Uri(ApiConfiguration.GetBackendHttpUrl("/api/v1/login")), HTTPMethods.Post);
		request.ConnectTimeout = TimeSpan.FromSeconds(10.0);
		request.Timeout = TimeSpan.FromSeconds(15.0);
		request.AddHeader("Authorization", "Basic " + candidateCredential);
		AddCommonAuthorizationHeaders(request);
		request.Send();
		return request;
	}

	public static void ActivateReviewerBasicAuth(string validatedCredential)
	{
		reviewerBasicAuthSession.Activate(validatedCredential);
		UserGameSettings.IsSignedInWithBasicAuth = true;
		MessageKit<bool>.post(138, true);
	}

	public static void ClearReviewerBasicAuth()
	{
		bool hadCredential = reviewerBasicAuthSession.Clear();
		bool wasSignedIn = UserGameSettings.IsSignedInWithBasicAuth;
		UserGameSettings.IsSignedInWithBasicAuth = false;
		if (hadCredential || wasSignedIn)
		{
			MessageKit<bool>.post(138, false);
		}
	}

	public void AuthorizeRequest(HTTPRequest request)
	{
		string credential = GetReviewerBasicAuthCredential();
		string authorization = (UserGameSettings.IsSignedInWithBasicAuth && !string.IsNullOrEmpty(credential)) ? ("Basic " + credential) : (Unity.Services.Core.UnityServices.State == Unity.Services.Core.ServicesInitializationState.Initialized ? AuthenticationService.Instance.AccessToken : "");
		request.AddHeader("Authorization", authorization);
		AddCommonAuthorizationHeaders(request);
	}

	private void AddCommonAuthorizationHeaders(HTTPRequest request)
	{
		request.AddHeader(requestIdHeaderName, Guid.NewGuid().ToString());
		request.AddHeader("dev", SystemInfo.deviceUniqueIdentifier);
		request.AddHeader("X-Game-User", UserGameSettings.multiplayerUsername);
		request.AddHeader("Client-Version", "4");
		request.AddHeader("Game-Version", Application.version);
	}

	public void ConnectToTheGame(string gameId, int playerId, bool retry)
	{
		UserGameSettings.multiplayerGameId = gameId;
		string url = BuildWebSocketUrl(gameId, playerId);
		Debug.Log("Connecting to " + url);
		if (gameSocket != null && (gameSocket.State <= WebSocketStates.Open || gameSocket.State == WebSocketStates.Unknown))
		{
			gameSocket.Close(4001, "Closing game socket as there's another one being opened");
		}
		gameSocket = new WebSocket(new Uri(url), null, null);
		AuthorizeRequest(gameSocket.InternalRequest);
		gameSocket.OnError = delegate(WebSocket socket, string reason)
		{
			MultiplayerServer.SocketErrorEvent(retryFailures);
			Debug.Log("Disconnected from server. Retrying (" + retryFailures + "). Error - " + reason);
			MessageKit<bool>.post(104, true);
			isConnected = false;
			if (retryFailures < 16)
			{
				coroutine = StartCoroutine(DelayedReconnect(gameId, playerId));
			}
			else
			{
				Debug.Log("Server unreachable");
				MessageKit<string>.post(87, "Disconnected  " + reason);
			}
		};
		gameSocket.OnOpen = delegate
		{
			MessageKit<bool>.post(104, false);
			isConnected = true;
			retryFailures = 0;
			Debug.Log("Connected to the server");
			MessageKit.post(85);
		};
		gameSocket.StartPingThread = true;
		gameSocket.PingFrequency = TimeSpan.FromSeconds(2.0);
		gameSocket.Open();
		if (retry && websocket != null)
		{
			gameSocket.OnMessage = websocket.OnMessage;
			gameSocket.OnBinary = websocket.OnBinary;
		}
		else
		{
			gameSocket.OnBinary = OnJoinedReceived;
		}
		websocket = gameSocket;
	}

	private IEnumerator DelayedReconnect(string gameId, int playerId)
	{
		yield return Utils.GetDelay(1f);
		retryFailures++;
		ConnectToTheGame(gameId, playerId, true);
	}

	private void OnJoinedReceived(WebSocket webSocket, byte[] message)
	{
		if (message.Length > 1 && message[0] == 'J')
		{
			int room = message[1];
			string username = Encoding.UTF8.GetString(message.Skip(2).ToArray());
			Debug.Log("Received JOINED with room number " + room + ". Other player's username is " + username);
			UserGameSettings.roomNumber = room;
			UserGameSettings.multiplayerSecondPlayerUsername = username;
			MessageKit.post(86);
		}
	}

	private static void AddBinary(OnWebSocketBinaryDelegate handler)
	{
		if (websocket != null)
		{
			websocket.OnBinary = (OnWebSocketBinaryDelegate)Delegate.Combine(websocket.OnBinary, handler);
		}
	}

	private static string TextFrom(byte[] message, int start)
	{
		return Encoding.UTF8.GetString(message, start, message.Length - start);
	}

	public void RegisterOnHostMove(Action<HostTransforms> action)
	{
		Debug.Log("Registering action onMove");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length == 33 && message[0] == 'M')
			{
				float[] values = ConvertByteArrayToFloat(message.Skip(1).ToArray());
				action?.Invoke(new HostTransforms(Build(values, 0), new Vector3(values[5], values[6], values[7])));
			}
		});
	}

	private static float[] ConvertByteArrayToFloat(byte[] bytes)
	{
		if ((bytes.Length & 3) != 0)
		{
			throw new ArgumentException();
		}
		float[] values = new float[bytes.Length >> 2];
		for (int i = 0; i < values.Length; i++)
		{
			values[i] = BitConverter.ToSingle(bytes, i * 4);
		}
		return values;
	}

	private ObjectTransform Build(float[] f, int posStart)
	{
		return new ObjectTransform
		{
			position = new Vector3(f[posStart], f[posStart + 1], f[posStart + 2]),
			rotationY = f[posStart + 3],
			rotationW = f[posStart + 4]
		};
	}

	public void RegisterOnGuestMove(Action<ObjectTransform> action)
	{
		Debug.Log("Registering action onMove");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length == 21 && message[0] == 'M')
			{
				action?.Invoke(Build(ConvertByteArrayToFloat(message.Skip(1).ToArray()), 0));
			}
		});
	}

	public void SendFinishGame(GameFinishedReason reason)
	{
		if (websocket == null)
		{
			return;
		}
		Debug.Log("Sending FinishGame F" + reason);
		if (isConnected)
		{
			websocket.Send(new byte[2]
			{
				70,
				(byte)reason
			});
		}
		else
		{
			reasonToSentWhenReconnected = reason;
			MessageKit.addObserver(85, GameReconnected);
		}
	}

	public void SendGameInterrupted(GameInterruptedReason reason)
	{
		if (websocket != null)
		{
			Debug.Log("Sending InterruptedReason " + reason);
			websocket.Send(new byte[2]
			{
				88,
				(byte)reason
			});
		}
	}

	public void SendGameResumed(GameResumedReason reason)
	{
		if (websocket != null)
		{
			Debug.Log("Sending ResumedReason " + reason);
			websocket.Send(new byte[2]
			{
				90,
				(byte)reason
			});
		}
	}

	private void GameReconnected()
	{
		MessageKit.removeObserver(85, GameReconnected);
		SendFinishGame(reasonToSentWhenReconnected);
	}

	public void RegisterOnFinish(Action<bool, GameFinishedReason> action)
	{
		Debug.Log("Registering action onFinish");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length != 3 || message[0] != 'F')
			{
				return;
			}
			bool won = message[1] == 1;
			GameFinishedReason reason = (GameFinishedReason)message[2];
			Debug.Log("Received FINISH - " + (won ? "win" : "lost") + ", reason = " + reason);
			MessageKit<bool>.post(103, false);
			MessageKit<GameFinishedReason>.post(105, reason);
			int code = message[2];
			if (code == 1)
			{
				CustomSingleton<MultiplayerSettings>.Instance.CloseWebsocket();
				if (!UserGameSettings.userQuitToMenu)
				{
					action?.Invoke(won, (GameFinishedReason)1);
				}
			}
			else if (code == 3)
			{
				if (!UserGameSettings.isMultiplayerHost)
				{
					MessageKit.post(92);
				}
				action?.Invoke(won, (GameFinishedReason)3);
			}
			else
			{
				if (code == 6)
				{
					CloseWebsocket();
				}
				action?.Invoke(won, reason);
			}
		});
	}

	public void RegisterOnInterrupted(Action<GameInterruptedReason> action)
	{
		Debug.Log("Registering action onInterrupted");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length == 2 && message[0] == 'X')
			{
				Debug.Log((message[1] == 1) ? "Other player quit the app" : "Other player focus lost");
				action?.Invoke((GameInterruptedReason)message[1]);
			}
		});
	}

	public void RegisterOnResumed(Action<GameResumedReason> action)
	{
		Debug.Log("Registering action onResumed");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length == 2 && message[0] == 'Z')
			{
				if (message[1] == 0)
				{
					Debug.Log("Other player focus gained");
				}
				action?.Invoke((GameResumedReason)message[1]);
			}
		});
	}

	public void CloseWebsocket()
	{
		Clear();
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
		}
		if (websocket != null)
		{
			websocket.Close();
		}
	}

	public void SendHostMove(HostTransforms transforms)
	{
		if (websocket != null)
		{
			float[] values = getPositions(transforms.host).Concat(getPositions(transforms.enemy)).ToArray();
			websocket.Send(encodeToBinary(values, 77));
		}
	}

	private byte[] encodeToBinary(float[] objects, byte startByte)
	{
		byte[] data = new byte[(objects.Length << 2) | 1];
		data[0] = startByte;
		for (int i = 0; i < objects.Length; i++)
		{
			BitConverter.GetBytes(objects[i]).CopyTo(data, 1 + i * 4);
		}
		return data;
	}

	private float[] getPositions(ObjectTransform transform)
	{
		return new float[5]
		{
			transform.position.x,
			transform.position.y,
			transform.position.z,
			transform.rotationY,
			transform.rotationW
		};
	}

	private float[] getPositions(Vector3 position)
	{
		return new float[3] { position.x, position.y, position.z };
	}

	public void SendGuestMove(ObjectTransform transform)
	{
		if (websocket != null)
		{
			websocket.Send(encodeToBinary(getPositions(transform), 77));
		}
	}

	public void SendReady(GameData initialGameData)
	{
		string json = JsonUtility.ToJson(initialGameData);
		Debug.Log("Sending READY " + json);
		if (websocket != null)
		{
			websocket.Send("R" + json);
		}
	}

	public void SendReady()
	{
		Debug.Log("Sending READY");
		if (websocket != null)
		{
			websocket.Send("R");
		}
	}

	public void SendInteraction(float id, byte interactableId)
	{
		Debug.Log("Sending Interaction " + id + " for interactable id " + interactableId);
		if (websocket != null)
		{
			byte[] data = new byte[6];
			data[0] = 73;
			BitConverter.GetBytes(id).CopyTo(data, 1);
			data[5] = interactableId;
			websocket.Send(data);
		}
	}

	private static byte[] WithPrefix(string text, params byte[] prefix)
	{
		byte[] body = Encoding.UTF8.GetBytes(text);
		byte[] data = new byte[body.Length + prefix.Length];
		prefix.CopyTo(data, 0);
		body.CopyTo(data, prefix.Length);
		return data;
	}

	public void SendBagCollected(string id)
	{
		Debug.Log("Sending BagCollected " + id);
		if (websocket != null)
		{
			websocket.Send(WithPrefix(id, 67));
		}
	}

	public void SendTotemMinigameFinished(bool result, string totemId)
	{
		Debug.Log("Sending totem minigame result for totem '" + totemId + "' with return " + result);
		if (websocket != null)
		{
			websocket.Send(WithPrefix(totemId, 71, 84, (byte)(result ? 49 : 48)));
		}
	}

	public void RegisterOnTotemMinigameFinished(Action<bool, string> action)
	{
		Debug.Log("Registering action on totems minigame solved");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length >= 4 && message[0] == 'G' && message[1] == 'T')
			{
				bool result = message[2] == '1';
				string totem = TextFrom(message, 3);
				Debug.Log("Received totem minigame result from server for totem '" + totem + "' with result " + result);
				action?.Invoke(result, totem);
			}
		});
	}

	public void SendPortalTravel(bool walkIntoPortal, float duration)
	{
		if (websocket != null)
		{
			websocket.Send(WithPrefix(Utils.FloatToString(duration), 79, 80, (byte)(walkIntoPortal ? 49 : 48)));
		}
	}

	public void RegisterOnPortalTravel(Action<bool, float> action)
	{
		Debug.Log("Registering action on portal travel");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length >= 4 && message[0] == 'O' && message[1] == 'P')
			{
				float.TryParse(TextFrom(message, 3), NumberStyles.Float, CultureInfo.InvariantCulture, out float duration);
				action?.Invoke(message[2] == '1', duration);
			}
		});
	}

	public void SendGameObjectPlaced(byte gameObjectSymbol, string gameObjectPositionString)
	{
		if (websocket != null)
		{
			Debug.Log("Sending game object '" + (char)gameObjectSymbol + "' placed at position " + gameObjectPositionString);
			websocket.Send(WithPrefix(gameObjectPositionString, 79, gameObjectSymbol, 80));
		}
	}

	public void RegisterOnGameObjectPlaced(Action<byte, string> action)
	{
		Debug.Log("Registering action on game object placed");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length >= 4 && message[0] == 'O' && message[2] == 'P')
			{
				string position = TextFrom(message, 3);
				Debug.Log("Received game object '" + (char)message[1] + "' placed at position " + position);
				action?.Invoke(message[1], position);
			}
		});
	}

	public void SendGameObjectEnemyInteraction(byte gameObjectSymbol, string gameObjectPositionString)
	{
		if (websocket != null)
		{
			websocket.Send(WithPrefix(gameObjectPositionString, 79, gameObjectSymbol, 69));
		}
	}

	public void RegisterOnGameObjectEnemyInteraction(Action<byte, string> action)
	{
		Debug.Log("Registering action on game object enemy interaction");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length >= 4 && message[0] == 'O' && message[2] == 'E')
			{
				action?.Invoke(message[1], TextFrom(message, 3));
			}
		});
	}

	public void SendPotionEffectUpdate(byte effectSymbol, bool state)
	{
		if (websocket != null)
		{
			Debug.Log("Sending potion effect update for effect '" + (char)effectSymbol + "'; active: " + state);
			websocket.Send(new byte[3]
			{
				80,
				effectSymbol,
				(byte)(state ? 49 : 48)
			});
		}
	}

	public void RegisterOnPotionEffectUpdate(Action<byte, bool> action)
	{
		Debug.Log("Registering potion effect update");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length == 3 && message[0] == 'P')
			{
				action?.Invoke(message[1], message[2] == '1');
			}
		});
	}

	public void SendGameObjectActiveStateUpdateById(byte gameObjectSymbol, bool state, string gameObjectId)
	{
		if (websocket != null)
		{
			websocket.Send(WithPrefix(gameObjectId, 79, gameObjectSymbol, 65, (byte)(state ? 49 : 48)));
		}
	}

	public void RegisterOnGameObjectActiveStateUpdateById(Action<byte, bool, string> action)
	{
		Debug.Log("Registering action on game object active state update");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length > 3 && message[0] == 'O' && message[2] == 'A')
			{
				action?.Invoke(message[1], message[3] == '1', TextFrom(message, 4));
			}
		});
	}

	public void SendPortraitsMinigameSolved()
	{
		if (websocket != null)
		{
			websocket.Send(new byte[1] { 71 });
		}
	}

	public void RegisterOnPortraitsMinigameSolved(Action action)
	{
		Debug.Log("Registering action on portraits minigame solved");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length == 1 && message[0] == 'G')
			{
				action?.Invoke();
			}
		});
	}

	public void SendItemUsed(byte itemSymbol)
	{
		if (websocket != null)
		{
			websocket.Send(new byte[2] { 85, itemSymbol });
		}
	}

	public void SendEnemyActivated()
	{
		if (websocket != null)
		{
			websocket.Send(new byte[1] { 65 });
		}
	}

	public void SendChatMessage(int message)
	{
		if (websocket != null)
		{
			websocket.Send(new byte[2]
			{
				72,
				(byte)message
			});
		}
	}

	public void SendEnemyChase(bool isChasing)
	{
		if (websocket != null)
		{
			websocket.Send(new byte[2]
			{
				68,
				(byte)(isChasing ? 1 : 0)
			});
		}
	}

	public void SendEnemyStateChanged(byte enemy, byte state)
	{
		if (websocket != null)
		{
			websocket.Send(new byte[3] { 84, enemy, state });
		}
	}

	public void SendGameObjectMessage(string message)
	{
		Debug.Log("Sending enemy message" + message);
		if (websocket != null)
		{
			websocket.Send(WithPrefix(message, 79));
		}
	}

	public void RegisterOnEnemyActivated(Action action)
	{
		Debug.Log("Registering action onEnemyActivated");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length == 1 && message[0] == 'A')
			{
				action?.Invoke();
			}
		});
	}

	public void RegisterOnStart(Action<GameData> action)
	{
		Debug.Log("Registering action onStart");
		if (websocket != null)
		{
			websocket.OnMessage = (OnWebSocketMessageDelegate)Delegate.Combine(websocket.OnMessage, new OnWebSocketMessageDelegate(delegate(WebSocket socket, string message)
			{
				if (message != null && message.StartsWith("S"))
				{
					Debug.Log("Received START " + message);
					action?.Invoke(JsonUtility.FromJson<GameData>(message.Substring(1)));
				}
			}));
		}
	}

	public void RegisterOnInteraction(Action<string> action)
	{
		Debug.Log("Registering action OnInteraction");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length > 4 && message[0] == 'I')
			{
				action?.Invoke(BitConverter.ToSingle(message, 1).ToString(CultureInfo.InvariantCulture));
			}
		});
	}

	public void RegisterOnResult(Action<Results> action)
	{
		Debug.Log("Registering action OnResult");
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length == 3 && message[0] == 'R')
			{
				action?.Invoke(new Results(message[1], message[2]));
			}
		});
	}

	public void RegisterOnEnemyChase(Action<bool, string> action)
	{
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length == 2 && message[0] == 'D')
			{
				action?.Invoke(message[1] == 1, "");
			}
		});
	}

	public void RegisterOnEnemyStateChanged(Action<byte, byte> action)
	{
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length == 3 && message[0] == 'T')
			{
				action?.Invoke(message[1], message[2]);
			}
		});
	}

	public void RegisterOnGameObjectMessage(Action<string> action)
	{
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length > 0 && message[0] == 'O')
			{
				action?.Invoke(TextFrom(message, 1));
			}
		});
	}

	public void RegisterOnChatMessage(Action<int> action)
	{
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length == 2 && message[0] == 'H')
			{
				action?.Invoke(message[1]);
			}
		});
	}

	public void SendPlayAgainReady(bool isReady)
	{
		if (websocket != null)
		{
			Debug.Log("Send playAgain=" + isReady);
			websocket.Send(new byte[2]
			{
				66,
				(byte)(isReady ? 1 : 0)
			});
		}
	}

	public void RegisterOnPlayAgainReady(Action<bool> action)
	{
		AddBinary(delegate(WebSocket socket, byte[] message)
		{
			if (message.Length == 2 && message[0] == 'B')
			{
				bool ready = message[1] == 1;
				Debug.Log("Received playAgain=" + ready);
				action?.Invoke(ready);
				if (!ready)
				{
					Debug.Log("play again false");
					CloseWebsocket();
				}
			}
		});
	}

	public void Clear()
	{
		if (websocket != null)
		{
			websocket.OnOpen = delegate
			{
			};
			websocket.OnMessage = delegate
			{
			};
			websocket.OnError = delegate
			{
			};
			websocket.OnBinary = OnJoinedReceived;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		ClearReviewerBasicAuth();
		base.OnApplicationQuit();
	}
}
