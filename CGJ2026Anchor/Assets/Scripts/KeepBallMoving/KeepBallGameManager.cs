using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KeepBallMoving
{
    public sealed class KeepBallGameManager : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private MatchMode matchMode = MatchMode.Pve;
        [SerializeField] private KeyCode redPrimaryKey = KeyCode.Return;
        [SerializeField] private KeyCode redAlternatePrimaryKey = KeyCode.RightControl;
        [SerializeField] private KeyCode redPhantomKey = KeyCode.RightShift;
        [SerializeField] private KeyCode redAlternatePhantomKey = KeyCode.Slash;

        [Header("Field")]
        [SerializeField] private float fieldWidth = 30f;
        [SerializeField] private float fieldHeight = 17f;
        [SerializeField] private Sprite fieldSprite;
        [SerializeField] private string fieldSpriteResourcePath = "KeepBallMoving/BG2";
        [SerializeField] private Color fieldSpriteTint = Color.white;
        [SerializeField] private float wallThickness = 0.5f;
        [SerializeField] private float goalDepth = 1f;
        [SerializeField] private float goalHeight = 4.5f;

        [Header("Actors")]
        [SerializeField] private int playersPerTeam = 5;
        [SerializeField] private float playerRadius = 0.5f;
        [SerializeField] private float ballRadius = 0.28f;
        [SerializeField] private float catchRadius = 1.8f;
        [SerializeField] private float holdPointRadius = 0.18f;

        [Header("Prefabs")]
        [SerializeField] private GameObject fieldVisualPrefab;
        [SerializeField] private PlayerAgent bluePlayerPrefab;
        [SerializeField] private PlayerAgent redPlayerPrefab;
        [SerializeField] private PlayerAgent phantomPlayerPrefab;
        [SerializeField] private BallController ballPrefab;
        [SerializeField] private BallController phantomBallPrefab;
        [SerializeField] private ShockwaveEffect blueShockwavePrefab;
        [SerializeField] private ShockwaveEffect redShockwavePrefab;
        [SerializeField] private KeepBallTalentBadge talentBadgePrefab;
        [SerializeField] private GameObject goalEffectPrefab;

        [Header("Hold And Release")]
        [SerializeField] private float minHoldRadius = 0.9f;
        [SerializeField] private float holdAngularSpeed = 360f;
        [SerializeField] private float minReleaseSpeed = 9f;
        [SerializeField] private float maxReleaseSpeed = 18f;
        [SerializeField] private float maxChargeTime = 1f;
        [SerializeField] private float maxBallSpeed = 22f;

        [Header("Simple Movement")]
        [SerializeField] private float blueMoveSpeed = 2.4f;
        [SerializeField] private float redMoveSpeed = 2.8f;
        [SerializeField] private float forwardMoveSpeedMultiplier = 1.08f;
        [SerializeField] private float midfielderMoveSpeedMultiplier = 1f;
        [SerializeField] private float defenderMoveSpeedMultiplier = 0.88f;
        [SerializeField] private float holderMoveSpeedMultiplier = 0.35f;
        [SerializeField] private float separationRadius = 1.25f;
        [SerializeField] private float separationStrength = 1.1f;
        [SerializeField] private float redKickoffAutoHoldLockout = 1.2f;
        [SerializeField] private float redAutoHoldDelay = 0.55f;
        [SerializeField] private float redPassSpeed = 11.5f;
        [SerializeField] private float redReleaseSpeed = 13.5f;

        [Header("Red AI Difficulty")]
        [SerializeField, Range(0f, 1f)] private float redAiStrength = 0.6f;
        [SerializeField, Range(0f, 1f)] private float redAutoCatchSkill = 0.75f;
        [SerializeField, Range(0f, 1f)] private float redDecisionSkill = 0.62f;
        [SerializeField, Range(0f, 1f)] private float redPassSkill = 0.55f;
        [SerializeField, Range(0f, 1f)] private float redShotSkill = 0.45f;
        [SerializeField, Range(0f, 1f)] private float redKickPowerSkill = 0.65f;
        [SerializeField, Range(0f, 1f)] private float redMovementSkill = 0.8f;
        [SerializeField, Range(0f, 0.9f)] private float redMaxCatchRadiusPenalty = 0.3f;
        [SerializeField] private float redMaxReactionDelayPenalty = 0.45f;
        [SerializeField] private float redMaxHoldDelayJitter = 0.35f;
        [SerializeField, Range(0f, 1f)] private float redMaxDecisionMistakeChance = 0.22f;
        [SerializeField] private float redMaxPassAimError = 2.1f;
        [SerializeField] private float redMaxShotAimError = 3.2f;
        [SerializeField, Range(0f, 0.8f)] private float redMaxKickSpeedRandomness = 0.24f;

        [Header("Talents")]
        [SerializeField] private float phantomBallSpawnOffset = 0.75f;
        [SerializeField] private float phantomBallSpeedMultiplier = 0.92f;
        [SerializeField] private float phantomBallScatterAngle = 60f;
        [SerializeField] private int phantomBallMaxBounces = 3;
        [SerializeField] private float bigfootForwardCatchBonusPerStack = 0.1f;
        [SerializeField] private float shieldFieldRadius = 3.2f;
        [SerializeField] private float shieldFieldPushDistance = 5f;
        [SerializeField] private float passFieldRadius = 2.8f;
        [SerializeField] private float passFieldPushDistance = 1.8f;
        [SerializeField] private float shockwavePushDuration = 0.18f;
        [SerializeField] private float shockwaveEffectDuration = 0.35f;
        [SerializeField] private float phantomPlayerKickSpeed = 15f;
        [SerializeField] private float phantomPlayerHoldAngularSpeed = 420f;
        [SerializeField] private Color phantomPlayerColor = new Color(0.42f, 0.92f, 1f, 0.78f);

        [Header("Goal Presentation")]
        [SerializeField, Range(0.05f, 1f)] private float goalSlowTimeScale = 0.18f;
        [SerializeField] private float goalCameraMoveDuration = 0.28f;
        [SerializeField] private float goalCameraReturnDuration = 0.24f;
        [SerializeField] private float goalDisplayDuration = 0.8f;
        [SerializeField] private float goalZoomOrthographicSize = 3.8f;
        [SerializeField] private float goalEffectLifetime = 1.5f;

        private readonly List<PlayerAgent> bluePlayers = new List<PlayerAgent>();
        private readonly List<PlayerAgent> redPlayers = new List<PlayerAgent>();
        private readonly List<PlayerAgent> extraPlayers = new List<PlayerAgent>();
        private readonly List<BallController> phantomBalls = new List<BallController>();
        private readonly List<KeepBallTalentBadge> blueTalentBadges = new List<KeepBallTalentBadge>();
        private readonly List<KeepBallTalentBadge> redTalentBadges = new List<KeepBallTalentBadge>();
        private readonly int[] blueTalentCounts = new int[TalentTypeCount];
        private readonly int[] redTalentCounts = new int[TalentTypeCount];
        private readonly Sprite[] talentIconSprites = new Sprite[TalentTypeCount];

        private BallController ball;
        private Transform playersRoot;
        private Sprite squareSprite;
        private Sprite bluePlayerSprite;
        private Sprite redPlayerSprite;
        private Sprite controlRangeSprite;
        private Sprite holdPointSprite;
        private Sprite ballSprite;
        private Sprite centerRingSprite;
        private Canvas talentCanvas;
        private RectTransform blueTalentPanel;
        private RectTransform redTalentPanel;
        private Camera gameplayCamera;
        private PhysicsMaterial2D bounceMaterial;
        private Vector3 defaultCameraPosition;
        private float defaultCameraOrthographicSize;
        private float defaultFixedDeltaTime;
        private float chargeTime;
        private bool goalLocked;
        private int blueScore;
        private int redScore;
        private string stateText = "空格/A 抓球";
        private string messageText = "基础原型：空格/A 抓球，松开释放";
        private float messageUntil;
        private string goalBannerText = string.Empty;
        private Color goalBannerColor = Color.white;
        private float goalBannerUntilRealtime;
        private float autoControlCooldownUntil;
        private float redCurrentHoldDelay;
        private bool blueHoldChargeArmed;
        private bool redHoldChargeArmed;
        private int bluePhantomPlayerUsesRemaining;
        private int redPhantomPlayerUsesRemaining;
        private bool bluePhantomPlayerPassWindowActive;
        private bool redPhantomPlayerPassWindowActive;
        private PlayerAgent activePhantomPlayer;
        private Team activePhantomPlayerTeam = Team.Blue;
        private PlayerAgent lockedKickoffPlayer;
        private bool kickoffPlayerLocked;
        private Vector2 lockedKickoffPosition;
        private Team nextKickoffTeam = Team.Blue;
        private bool talentSelectionOpen;
        private int selectedTalentIndex;
        private float nextTalentMoveInputTime;
        private bool rightStickHorizontalMissing;
        private bool showInputDebug;
        private readonly HashSet<string> missingInputAxes = new HashSet<string>();
        private readonly TalentOption[] currentTalentOptions = new TalentOption[TalentChoiceCount];
        private readonly bool[] talentOptionTaken = new bool[TalentChoiceCount];
        private Team talentFirstPickTeam = Team.Blue;
        private Team talentSecondPickTeam = Team.Red;
        private Team currentTalentPickingTeam = Team.Blue;
        private bool talentSecondPickPending;
        private string lastBlueTalentText = "暂无";
        private string lastRedTalentText = "暂无";

        private Color fieldColor = new Color(0.08f, 0.42f, 0.19f, 1f);
        private Color lineColor = new Color(0.93f, 0.97f, 0.93f, 1f);

        private const string DefaultPlayerPrefabPath = "KeepBallMoving/KeepBallPlayer";
        private const string DefaultRedPlayerPrefabPath = "KeepBallMoving/KeepBallPlayer1";
        private const string DefaultPhantomPlayerPrefabPath = "KeepBallMoving/KeepBallPhantomPlayer";
        private const string DefaultBallPrefabPath = "KeepBallMoving/KeepBallBall";
        private const string DefaultPhantomBallPrefabPath = "KeepBallMoving/KeepBallPhantomBall";
        private const string DefaultBlueShockwavePrefabPath = "KeepBallMoving/KeepBallBlueShockwave";
        private const string DefaultRedShockwavePrefabPath = "KeepBallMoving/KeepBallRedShockwave";
        private const string DefaultTalentBadgePrefabPath = "KeepBallMoving/KeepBallTalentBadge";
        private const string DefaultGoalEffectPrefabPath = "KeepBallMoving/GoalEffect";
        private const string DefaultFieldVisualPrefabPath = "KeepBallMoving/KeepBallField";
        private const string DefaultFieldSpritePath = "KeepBallMoving/BG2";
        private const float TalentBadgeWidth = 246f;
        private const float TalentBadgeHeight = 44f;
        private const float TalentBadgeGap = 6f;
        private const int TalentChoiceCount = 3;
        private const int TalentTypeCount = 9;

        private enum MatchMode
        {
            Pve,
            Pvp
        }

        private enum TalentId
        {
            PhantomFootball,
            BigfootForward,
            BananaBall,
            ExtraForward,
            ExtraMidfielder,
            ExtraDefender,
            ShieldField,
            PassField,
            PhantomPlayer
        }

        private struct TalentOption
        {
            public TalentId Id;
            public string Name;
            public string Description;
            public Color AccentColor;
        }

        private void Awake()
        {
            Physics2D.gravity = Vector2.zero;
            defaultFixedDeltaTime = Time.fixedDeltaTime;
            LoadDefaultPrefabs();
            CreateRuntimeAssets();
            SetupCamera();
            BuildField();
            SpawnPlayers();
            SpawnBall();
            SetupTalentUi();
            ResetRound();
        }

        private void Update()
        {
            if (ball == null)
            {
                return;
            }

            if (talentSelectionOpen)
            {
                HandleTalentSelectionInput();
                return;
            }

            if (goalLocked)
            {
                return;
            }

            UpdatePlayerMovement(Time.deltaTime);
            UpdatePhantomBallInterceptions();

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetRound();
                return;
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                RestartMatch();
                return;
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleMatchMode();
                return;
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                showInputDebug = !showInputDebug;
            }

            if (ball.State == BallState.Held)
            {
                ball.TickHold(Time.deltaTime);

                if (activePhantomPlayer != null && ball.Holder == activePhantomPlayer)
                {
                    stateText = $"{GetTeamLabel(activePhantomPlayerTeam)}幻影球员控球：按{GetPrimaryActionHint(activePhantomPlayerTeam)}踢出";

                    if (IsPrimaryActionDown(activePhantomPlayerTeam))
                    {
                        ReleasePhantomPlayerBall();
                    }

                    return;
                }

                if (ball.Holder != null && ball.Holder.Team == Team.Red)
                {
                    if (IsPvpMode())
                    {
                        HandlePlayerHeldBall(Team.Red, Time.deltaTime);
                    }
                    else
                    {
                        HandleRedAiHeldBall(Time.deltaTime);
                    }

                    return;
                }

                HandlePlayerHeldBall(Team.Blue, Time.deltaTime);
                return;
            }

            if (IsPhantomActionDown(Team.Blue) && TryActivatePhantomPlayer(Team.Blue))
            {
                return;
            }

            if (IsPvpMode() && IsPhantomActionDown(Team.Red) && TryActivatePhantomPlayer(Team.Red))
            {
                return;
            }

            UpdateControlHighlights();
            bool blueCanCatch = FindNearestCatchablePlayer(Team.Blue) != null;
            bool redCanCatch = IsPvpMode() && FindNearestCatchablePlayer(Team.Red) != null;
            stateText = GetFreeBallStateText(blueCanCatch, redCanCatch);

            if (IsPrimaryActionDown(Team.Blue))
            {
                TryCatchBall(Team.Blue);
                return;
            }

            if (IsPvpMode() && IsPrimaryActionDown(Team.Red))
            {
                TryCatchBall(Team.Red);
                return;
            }

            if (!IsPvpMode() && TryAutoRedCatchBall())
            {
                return;
            }

        }

        private bool IsPrimaryActionDown()
        {
            return IsPrimaryActionDown(Team.Blue);
        }

        private bool IsPrimaryActionDown(Team team)
        {
            if (KeepBallNewInput.IsPrimaryDown(GetGamepadIndex(team), ShouldReadAnyGamepad(team)))
            {
                return true;
            }

            if (team == Team.Red)
            {
                return Input.GetKeyDown(redPrimaryKey) ||
                    Input.GetKeyDown(redAlternatePrimaryKey) ||
                    KeepBallNewInput.IsLegacyPrimaryDown(GetGamepadIndex(team), false);
            }

            return Input.GetKeyDown(KeyCode.Space) ||
                KeepBallNewInput.IsLegacyPrimaryDown(GetGamepadIndex(team), ShouldReadLegacyAnyGamepad(team));
        }

        private bool IsPrimaryActionUp(Team team)
        {
            if (KeepBallNewInput.IsPrimaryUp(GetGamepadIndex(team), ShouldReadAnyGamepad(team)))
            {
                return true;
            }

            if (team == Team.Red)
            {
                return Input.GetKeyUp(redPrimaryKey) ||
                    Input.GetKeyUp(redAlternatePrimaryKey) ||
                    KeepBallNewInput.IsLegacyPrimaryUp(GetGamepadIndex(team), false);
            }

            return Input.GetKeyUp(KeyCode.Space) ||
                KeepBallNewInput.IsLegacyPrimaryUp(GetGamepadIndex(team), ShouldReadLegacyAnyGamepad(team));
        }

        private bool IsPhantomActionDown(Team team)
        {
            if (KeepBallNewInput.IsPhantomDown(GetGamepadIndex(team), ShouldReadAnyGamepad(team)))
            {
                return true;
            }

            if (team == Team.Red)
            {
                return Input.GetKeyDown(redPhantomKey) ||
                    Input.GetKeyDown(redAlternatePhantomKey) ||
                    KeepBallNewInput.IsLegacyPhantomDown(GetGamepadIndex(team), false);
            }

            return Input.GetKeyDown(KeyCode.Z) ||
                KeepBallNewInput.IsLegacyPhantomDown(GetGamepadIndex(team), ShouldReadLegacyAnyGamepad(team));
        }

        private int GetGamepadIndex(Team team)
        {
            return team == Team.Blue ? 0 : 1;
        }

        private bool ShouldReadAnyGamepad(Team team)
        {
            return team == Team.Blue && !IsPvpMode();
        }

        private bool ShouldReadLegacyAnyGamepad(Team team)
        {
            return team == Team.Blue && !IsPvpMode();
        }

        private bool IsPvpMode()
        {
            return matchMode == MatchMode.Pvp;
        }

        private string GetMatchModeLabel()
        {
            return IsPvpMode() ? "PVP" : "PVE";
        }

        private void HandlePlayerHeldBall(Team team, float deltaTime)
        {
            if (!IsHoldChargeArmed(team))
            {
                stateText = $"{GetTeamLabel(team)}持球：按住{GetPrimaryActionHint(team)}蓄力";

                if (IsPrimaryActionDown(team))
                {
                    SetHoldChargeArmed(team, true);
                    chargeTime = 0f;
                    ShowMessage($"{GetTeamLabel(team)}蓄力中");
                }

                return;
            }

            chargeTime += deltaTime;
            stateText = $"{GetTeamLabel(team)}蓄力 {Mathf.Clamp01(chargeTime / maxChargeTime):P0}";

            if (IsPrimaryActionUp(team))
            {
                ReleasePlayerHeldBall();
            }
        }

        private void HandleRedAiHeldBall(float deltaTime)
        {
            chargeTime += deltaTime;
            float holdDelay = Mathf.Max(0.05f, redCurrentHoldDelay);
            stateText = $"红方控球 {Mathf.Clamp01(chargeTime / holdDelay):P0}";

            if (chargeTime >= holdDelay)
            {
                ReleaseRedHeldBall();
            }
        }

        private string GetPrimaryActionHint(Team team)
        {
            return team == Team.Red ? "Enter/右Ctrl/手柄2A" : "空格/手柄1A";
        }

        private string GetPhantomActionHint(Team team)
        {
            return team == Team.Red ? "右Shift或/或手柄2B" : "Z/手柄1B";
        }

        private string GetFreeBallStateText(bool blueCanCatch, bool redCanCatch)
        {
            if (IsPvpMode())
            {
                if (blueCanCatch && redCanCatch)
                {
                    return "双方都可抓球";
                }

                if (blueCanCatch)
                {
                    return "蓝方可抓球：空格/手柄1A";
                }

                if (redCanCatch)
                {
                    return "红方可抓球：Enter/右Ctrl/手柄2A";
                }

                return "等待球靠近任一方";
            }

            return blueCanCatch ? "可抓球：按空格/手柄1A" : "等待球靠近蓝方";
        }

        private void HandleTalentSelectionInput()
        {
            int direction = GetTalentSelectionDirection(currentTalentPickingTeam);
            if (direction != 0 && Time.unscaledTime >= nextTalentMoveInputTime)
            {
                selectedTalentIndex = FindNextSelectableTalentIndex(selectedTalentIndex, direction, currentTalentPickingTeam);
                nextTalentMoveInputTime = Time.unscaledTime + 0.18f;
            }

            if (IsTalentConfirmDown())
            {
                SelectTalent(selectedTalentIndex);
            }
        }

        private bool IsTalentConfirmDown()
        {
            if (IsPrimaryActionDown(currentTalentPickingTeam))
            {
                return true;
            }

            return currentTalentPickingTeam != Team.Blue && IsPrimaryActionDown(Team.Blue);
        }

        private int GetTalentSelectionDirection(Team team)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                return -1;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                return 1;
            }

            float axis = GetTeamHorizontalAxis(team);
            if (Mathf.Abs(axis) < 0.55f)
            {
                axis = GetRightStickHorizontal();
            }

            if (axis <= -0.55f)
            {
                return -1;
            }

            if (axis >= 0.55f)
            {
                return 1;
            }

            nextTalentMoveInputTime = 0f;
            return 0;
        }

        private int FindNextSelectableTalentIndex(int startIndex, int direction, Team team)
        {
            int safeDirection = direction >= 0 ? 1 : -1;
            int index = Mathf.Clamp(startIndex, 0, TalentChoiceCount - 1);
            for (int step = 0; step < TalentChoiceCount; step++)
            {
                index = Mathf.Clamp(index + safeDirection, 0, TalentChoiceCount - 1);
                if (CanSelectTalentOption(index, team))
                {
                    return index;
                }

                if ((safeDirection < 0 && index == 0) || (safeDirection > 0 && index == TalentChoiceCount - 1))
                {
                    break;
                }
            }

            return CanSelectTalentOption(startIndex, team) ? startIndex : FindFirstSelectableTalentIndex(team);
        }

        private float GetAxisRawSafe(string axisName)
        {
            if (missingInputAxes.Contains(axisName))
            {
                return 0f;
            }

            try
            {
                return Input.GetAxisRaw(axisName);
            }
            catch (System.ArgumentException)
            {
                missingInputAxes.Add(axisName);
                return 0f;
            }
        }

        private float GetTeamHorizontalAxis(Team team)
        {
            float axis = KeepBallNewInput.GetHorizontal(GetGamepadIndex(team), ShouldReadAnyGamepad(team));
            if (Mathf.Abs(axis) >= 0.55f)
            {
                return axis;
            }

            axis = GetAxisRawSafe(team == Team.Blue ? "Joystick1Horizontal" : "Joystick2Horizontal");
            if (Mathf.Abs(axis) >= 0.55f)
            {
                return axis;
            }

            axis = GetAxisRawSafe(team == Team.Blue ? "P1Horizontal" : "P2Horizontal");
            if (Mathf.Abs(axis) >= 0.55f)
            {
                return axis;
            }

            return GetAxisRawSafe("Horizontal");
        }

        private float GetRightStickHorizontal()
        {
            if (rightStickHorizontalMissing)
            {
                return 0f;
            }

            try
            {
                return Input.GetAxisRaw("RightStickHorizontal");
            }
            catch (System.ArgumentException)
            {
                rightStickHorizontalMissing = true;
                return 0f;
            }
        }

        private void CreateRuntimeAssets()
        {
            squareSprite = RuntimeSpriteFactory.CreateSquareSprite("KeepBall_Square", Color.white);
            bluePlayerSprite = RuntimeSpriteFactory.CreateCircleSprite("KeepBall_BluePlayer", Color.white);
            redPlayerSprite = RuntimeSpriteFactory.CreateCircleSprite("KeepBall_RedPlayer", Color.white);
            controlRangeSprite = RuntimeSpriteFactory.CreateRingSprite("KeepBall_ControlRange", Color.white, 256, 0.035f);
            holdPointSprite = RuntimeSpriteFactory.CreateCircleSprite("KeepBall_HoldPoint", Color.white, 96);
            ballSprite = RuntimeSpriteFactory.CreateCircleSprite("KeepBall_Ball", Color.white);
            centerRingSprite = RuntimeSpriteFactory.CreateRingSprite("KeepBall_CenterRing", lineColor, 256, 0.035f);
            CreateTalentIconSprites();

            bounceMaterial = new PhysicsMaterial2D("KeepBall_Bouncy")
            {
                friction = 0f,
                bounciness = 0.88f
            };
        }

        private void CreateTalentIconSprites()
        {
            for (int i = 0; i < TalentTypeCount; i++)
            {
                TalentId talentId = (TalentId)i;
                talentIconSprites[i] = RuntimeSpriteFactory.CreateTalentIconSprite(
                    $"KeepBall_TalentIcon_{talentId}",
                    GetTalentColor(talentId),
                    i);
            }
        }

        private void LoadDefaultPrefabs()
        {
            PlayerAgent defaultPlayerPrefab = null;
            PlayerAgent defaultRedPlayerPrefab = null;

            if (bluePlayerPrefab == null || redPlayerPrefab == null)
            {
                defaultPlayerPrefab = Resources.Load<PlayerAgent>(DefaultPlayerPrefabPath);
            }

            if (redPlayerPrefab == null)
            {
                defaultRedPlayerPrefab = Resources.Load<PlayerAgent>(DefaultRedPlayerPrefabPath);
            }

            if (bluePlayerPrefab == null)
            {
                bluePlayerPrefab = defaultPlayerPrefab;
            }

            if (redPlayerPrefab == null)
            {
                redPlayerPrefab = defaultRedPlayerPrefab != null ? defaultRedPlayerPrefab : defaultPlayerPrefab;
            }

            if (phantomPlayerPrefab == null)
            {
                phantomPlayerPrefab = Resources.Load<PlayerAgent>(DefaultPhantomPlayerPrefabPath);
            }

            if (ballPrefab == null)
            {
                ballPrefab = Resources.Load<BallController>(DefaultBallPrefabPath);
            }

            if (phantomBallPrefab == null)
            {
                phantomBallPrefab = Resources.Load<BallController>(DefaultPhantomBallPrefabPath);
            }

            if (blueShockwavePrefab == null)
            {
                blueShockwavePrefab = Resources.Load<ShockwaveEffect>(DefaultBlueShockwavePrefabPath);
            }

            if (redShockwavePrefab == null)
            {
                redShockwavePrefab = Resources.Load<ShockwaveEffect>(DefaultRedShockwavePrefabPath);
            }

            if (talentBadgePrefab == null)
            {
                talentBadgePrefab = Resources.Load<KeepBallTalentBadge>(DefaultTalentBadgePrefabPath);
            }

            if (goalEffectPrefab == null)
            {
                goalEffectPrefab = Resources.Load<GameObject>(DefaultGoalEffectPrefabPath);
            }

            if (fieldVisualPrefab == null)
            {
                fieldVisualPrefab = Resources.Load<GameObject>(DefaultFieldVisualPrefabPath);
            }

            if (fieldVisualPrefab == null && fieldSprite == null)
            {
                string spritePath = string.IsNullOrWhiteSpace(fieldSpriteResourcePath) ? DefaultFieldSpritePath : fieldSpriteResourcePath;
                fieldSprite = Resources.Load<Sprite>(spritePath);
            }
        }

        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            mainCamera.transform.rotation = Quaternion.identity;
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = fieldHeight * 0.58f;
            mainCamera.backgroundColor = new Color(0.04f, 0.11f, 0.08f, 1f);
            gameplayCamera = mainCamera;
            defaultCameraPosition = mainCamera.transform.position;
            defaultCameraOrthographicSize = 12;
        }

        private void BuildField()
        {
            GameObject fieldRoot = new GameObject("Field");
            fieldRoot.transform.SetParent(transform);

            CreateFieldVisual(fieldRoot.transform);
            CreateVisualRect("CenterLine", Vector2.zero, new Vector2(0.06f, fieldHeight), lineColor, 0, fieldRoot.transform);
            CreateVisualRect("HalfwayMark", Vector2.zero, new Vector2(0.25f, 0.25f), lineColor, 1, fieldRoot.transform);
            CreateVisualRect("TopLine", new Vector2(0f, fieldHeight * 0.5f), new Vector2(fieldWidth, 0.06f), lineColor, 0, fieldRoot.transform);
            CreateVisualRect("BottomLine", new Vector2(0f, -fieldHeight * 0.5f), new Vector2(fieldWidth, 0.06f), lineColor, 0, fieldRoot.transform);
            CreateVisualRect("LeftLine", new Vector2(-fieldWidth * 0.5f, 0f), new Vector2(0.06f, fieldHeight), lineColor, 0, fieldRoot.transform);
            CreateVisualRect("RightLine", new Vector2(fieldWidth * 0.5f, 0f), new Vector2(0.06f, fieldHeight), lineColor, 0, fieldRoot.transform);
            CreateCenterRing(fieldRoot.transform);
            CreatePenaltyBox("LeftPenaltyBox", -1f, fieldRoot.transform);
            CreatePenaltyBox("RightPenaltyBox", 1f, fieldRoot.transform);

            CreateWall("TopWall", new Vector2(0f, fieldHeight * 0.5f + wallThickness * 0.5f), new Vector2(fieldWidth + wallThickness * 2f, wallThickness), fieldRoot.transform);
            CreateWall("BottomWall", new Vector2(0f, -fieldHeight * 0.5f - wallThickness * 0.5f), new Vector2(fieldWidth + wallThickness * 2f, wallThickness), fieldRoot.transform);

            float sideWallHeight = (fieldHeight - goalHeight) * 0.5f;
            float upperSideY = goalHeight * 0.5f + sideWallHeight * 0.5f;
            float lowerSideY = -upperSideY;
            CreateWall("LeftUpperWall", new Vector2(-fieldWidth * 0.5f - wallThickness * 0.5f, upperSideY), new Vector2(wallThickness, sideWallHeight), fieldRoot.transform);
            CreateWall("LeftLowerWall", new Vector2(-fieldWidth * 0.5f - wallThickness * 0.5f, lowerSideY), new Vector2(wallThickness, sideWallHeight), fieldRoot.transform);
            CreateWall("RightUpperWall", new Vector2(fieldWidth * 0.5f + wallThickness * 0.5f, upperSideY), new Vector2(wallThickness, sideWallHeight), fieldRoot.transform);
            CreateWall("RightLowerWall", new Vector2(fieldWidth * 0.5f + wallThickness * 0.5f, lowerSideY), new Vector2(wallThickness, sideWallHeight), fieldRoot.transform);

            CreateGoal(GoalSide.Left, new Vector2(-fieldWidth * 0.5f - goalDepth * 0.5f, 0f), fieldRoot.transform);
            CreateGoal(GoalSide.Right, new Vector2(fieldWidth * 0.5f + goalDepth * 0.5f, 0f), fieldRoot.transform);
        }

        private void CreateFieldBackground(Transform parent)
        {
            Sprite backgroundSprite = fieldSprite != null ? fieldSprite : squareSprite;
            Color backgroundColor = fieldSprite != null ? fieldSpriteTint : fieldColor;
            CreateVisualRect("Grass", Vector2.zero, new Vector2(fieldWidth, fieldHeight), backgroundColor, -10, parent, backgroundSprite);
        }

        private void CreateFieldVisual(Transform parent)
        {
            if (fieldVisualPrefab != null)
            {
                GameObject visual = Instantiate(fieldVisualPrefab, parent);
                visual.name = "KeepBallField";
                return;
            }

            CreateFieldBackground(parent);
        }

        private void CreatePenaltyBox(string name, float direction, Transform parent)
        {
            float boxWidth = 4.2f;
            float boxHeight = 7.2f;
            float x = direction * (fieldWidth * 0.5f - boxWidth * 0.5f);

            CreateVisualRect($"{name}_Back", new Vector2(direction * fieldWidth * 0.5f, 0f), new Vector2(0.06f, boxHeight), lineColor, 1, parent);
            CreateVisualRect($"{name}_Front", new Vector2(direction * (fieldWidth * 0.5f - boxWidth), 0f), new Vector2(0.06f, boxHeight), lineColor, 1, parent);
            CreateVisualRect($"{name}_Top", new Vector2(x, boxHeight * 0.5f), new Vector2(boxWidth, 0.06f), lineColor, 1, parent);
            CreateVisualRect($"{name}_Bottom", new Vector2(x, -boxHeight * 0.5f), new Vector2(boxWidth, 0.06f), lineColor, 1, parent);
        }

        private void CreateCenterRing(Transform parent)
        {
            GameObject ring = new GameObject("CenterCircle");
            ring.transform.SetParent(parent);
            ring.transform.position = Vector3.zero;
            ring.transform.localScale = Vector3.one * 4.4f;

            SpriteRenderer renderer = ring.AddComponent<SpriteRenderer>();
            renderer.sprite = centerRingSprite;
            renderer.color = lineColor;
            renderer.sortingOrder = 1;
        }

        private GameObject CreateVisualRect(string name, Vector2 position, Vector2 size, Color color, int sortingOrder, Transform parent, Sprite spriteOverride = null)
        {
            GameObject rect = new GameObject(name);
            rect.transform.SetParent(parent);
            rect.transform.position = position;

            SpriteRenderer renderer = rect.AddComponent<SpriteRenderer>();
            renderer.sprite = spriteOverride != null ? spriteOverride : squareSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            Vector2 spriteSize = renderer.sprite != null ? renderer.sprite.bounds.size : Vector2.one;
            rect.transform.localScale = new Vector3(
                size.x / Mathf.Max(0.001f, spriteSize.x),
                size.y / Mathf.Max(0.001f, spriteSize.y),
                1f);

            return rect;
        }

        private void CreateWall(string name, Vector2 position, Vector2 size, Transform parent)
        {
            GameObject wall = CreateVisualRect(name, position, size, lineColor, 2, parent);
            BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.sharedMaterial = bounceMaterial;
        }

        private void CreateGoal(GoalSide side, Vector2 position, Transform parent)
        {
            string sideName = side == GoalSide.Left ? "Left" : "Right";
            GameObject goal = CreateVisualRect($"{sideName}GoalTrigger", position, new Vector2(goalDepth, goalHeight), new Color(1f, 1f, 1f, 0.18f), 3, parent);
            GoalTrigger trigger = goal.AddComponent<GoalTrigger>();
            trigger.Initialize(this, side, new Vector2(goalDepth, goalHeight));

            float sign = side == GoalSide.Left ? -1f : 1f;
            CreateVisualRect($"{sideName}GoalBack", new Vector2(sign * (fieldWidth * 0.5f + goalDepth), 0f), new Vector2(0.06f, goalHeight), lineColor, 4, parent);
            CreateVisualRect($"{sideName}GoalTop", new Vector2(sign * (fieldWidth * 0.5f + goalDepth * 0.5f), goalHeight * 0.5f), new Vector2(goalDepth, 0.06f), lineColor, 4, parent);
            CreateVisualRect($"{sideName}GoalBottom", new Vector2(sign * (fieldWidth * 0.5f + goalDepth * 0.5f), -goalHeight * 0.5f), new Vector2(goalDepth, 0.06f), lineColor, 4, parent);
        }

        private void SpawnPlayers()
        {
            GameObject playersRootObject = new GameObject("Players");
            playersRootObject.transform.SetParent(transform);
            playersRoot = playersRootObject.transform;

            for (int i = 0; i < playersPerTeam; i++)
            {
                PlayerRole role = GetRoleForIndex(i, playersPerTeam);
                Vector2 bluePosition = GetPlayerSpawnPosition(Team.Blue, role, i, playersPerTeam);
                PlayerAgent blue = CreatePlayer(Team.Blue, role, i + 1, bluePosition, playersRoot);
                bluePlayers.Add(blue);

                Vector2 redPosition = GetPlayerSpawnPosition(Team.Red, role, i, playersPerTeam);
                PlayerAgent red = CreatePlayer(Team.Red, role, i + 1, redPosition, playersRoot);
                redPlayers.Add(red);
            }
        }

        private PlayerRole GetRoleForIndex(int index, int total)
        {
            int defenderCount = Mathf.Max(1, total / 4);
            int forwardCount = Mathf.Max(1, Mathf.CeilToInt(total / 3f));

            if (index < defenderCount)
            {
                return PlayerRole.Defender;
            }

            if (index >= total - forwardCount)
            {
                return PlayerRole.Forward;
            }

            return PlayerRole.Midfielder;
        }

        private Vector2 GetPlayerSpawnPosition(Team team, PlayerRole role, int index, int total)
        {
            float side = team == Team.Blue ? -1f : 1f;
            float xRatio = role == PlayerRole.Defender ? 0.36f : role == PlayerRole.Midfielder ? 0.12f : -0.16f;
            float x = side * fieldWidth * xRatio;
            float y = GetRoleLaneForIndex(index, total, role, fieldHeight * 0.34f);
            return new Vector2(x, y);
        }

        private Vector2 GetExtraPlayerSpawnPosition(Team team, PlayerRole role)
        {
            List<PlayerAgent> teamPlayers = team == Team.Blue ? bluePlayers : redPlayers;
            int roleSlot = CountRole(teamPlayers, role);
            int roleCount = roleSlot + 1;
            float lane = GetLaneForRoleSlot(roleSlot, roleCount, fieldHeight * 0.34f);

            float rightSideX = fieldWidth * 0.22f;
            float teamSide = team == Team.Blue ? -1f : 1f;
            float fallbackRoleX = teamSide * fieldWidth * (role == PlayerRole.Defender ? 0.36f : role == PlayerRole.Midfielder ? 0.12f : -0.16f);
            float x = Mathf.Lerp(fallbackRoleX, rightSideX, 0.65f);

            return ClampInsideField(new Vector2(x, lane));
        }

        private Vector2 GetResetFormationPosition(Team team, PlayerAgent player)
        {
            List<PlayerAgent> teamPlayers = team == Team.Blue ? bluePlayers : redPlayers;
            int roleSlot = GetRoleSlot(teamPlayers, player);
            int roleCount = CountRole(teamPlayers, player.Role);
            float lane = GetLaneForRoleSlot(roleSlot, roleCount, fieldHeight * 0.34f);

            float x;
            if (extraPlayers.Contains(player))
            {
                float rightSideX = fieldWidth * 0.22f;
                float teamSide = team == Team.Blue ? -1f : 1f;
                float fallbackRoleX = teamSide * fieldWidth * GetRoleFormationRatio(player.Role);
                x = Mathf.Lerp(fallbackRoleX, rightSideX, 0.65f);
            }
            else
            {
                float side = team == Team.Blue ? -1f : 1f;
                x = side * fieldWidth * GetRoleFormationRatio(player.Role);
            }

            return ClampInsideField(new Vector2(x, lane));
        }

        private float GetRoleFormationRatio(PlayerRole role)
        {
            return role == PlayerRole.Defender ? 0.36f : role == PlayerRole.Midfielder ? 0.12f : -0.16f;
        }

        private void ResetTeamToFormation(Team team)
        {
            List<PlayerAgent> teamPlayers = team == Team.Blue ? bluePlayers : redPlayers;
            foreach (PlayerAgent player in teamPlayers)
            {
                player.ResetToPosition(GetResetFormationPosition(team, player));
                SyncPlayerControlRangeVisual(player);
            }
        }

        private PlayerAgent AddExtraPlayer(Team team, PlayerRole role)
        {
            if (playersRoot == null)
            {
                playersRoot = transform;
            }

            List<PlayerAgent> teamPlayers = team == Team.Blue ? bluePlayers : redPlayers;
            Vector2 position = GetExtraPlayerSpawnPosition(team, role);
            PlayerAgent player = CreatePlayer(team, role, teamPlayers.Count + 1, position, playersRoot);
            teamPlayers.Add(player);
            extraPlayers.Add(player);
            SyncPlayerControlRangeVisual(player);
            return player;
        }

        private PlayerAgent CreatePlayer(Team team, PlayerRole role, int index, Vector2 position, Transform parent)
        {
            PlayerAgent prefab = team == Team.Blue ? bluePlayerPrefab : redPlayerPrefab;
            PlayerAgent player;

            if (prefab != null)
            {
                player = Instantiate(prefab, position, Quaternion.identity, parent);
            }
            else
            {
                GameObject playerObject = new GameObject();
                playerObject.transform.SetParent(parent);
                playerObject.transform.position = position;
                playerObject.AddComponent<SpriteRenderer>();
                playerObject.AddComponent<CircleCollider2D>();
                player = playerObject.AddComponent<PlayerAgent>();
            }

            player.Initialize(
                team,
                role,
                index,
                position,
                team == Team.Blue ? bluePlayerSprite : redPlayerSprite,
                controlRangeSprite,
                holdPointSprite,
                playerRadius,
                catchRadius,
                holdPointRadius);
            SyncPlayerControlRangeVisual(player);

            return player;
        }

        private void SpawnBall()
        {
            if (ballPrefab != null)
            {
                ball = Instantiate(ballPrefab, transform);
            }
            else
            {
                GameObject ballObject = new GameObject("Ball");
                ballObject.transform.SetParent(transform);
                ballObject.AddComponent<SpriteRenderer>();
                ballObject.AddComponent<Rigidbody2D>();
                ballObject.AddComponent<CircleCollider2D>();
                ball = ballObject.AddComponent<BallController>();
            }

            ball.Initialize(ballSprite, bounceMaterial, ballRadius, maxBallSpeed);
        }

        private void SetupTalentUi()
        {
            if (talentCanvas != null)
            {
                UpdateTalentUi();
                return;
            }

            GameObject canvasObject = new GameObject("KeepBallTalentUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            talentCanvas = canvasObject.GetComponent<Canvas>();
            talentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            talentCanvas.sortingOrder = 45;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            blueTalentPanel = CreateTalentPanel("BlueTalentPanel", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(18f, 18f));
            redTalentPanel = CreateTalentPanel("RedTalentPanel", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-18f, 18f));
            UpdateTalentUi();
        }

        private RectTransform CreateTalentPanel(string panelName, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition)
        {
            GameObject panelObject = new GameObject(panelName, typeof(RectTransform));
            panelObject.transform.SetParent(talentCanvas.transform, false);

            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(TalentBadgeWidth, TalentBadgeHeight * TalentTypeCount + TalentBadgeGap * (TalentTypeCount - 1));
            return rectTransform;
        }

        private void UpdateTalentUi()
        {
            if (talentCanvas == null || blueTalentPanel == null || redTalentPanel == null)
            {
                return;
            }

            RefreshTalentBadges(Team.Blue);
            RefreshTalentBadges(Team.Red);
        }

        private void RefreshTalentBadges(Team team)
        {
            List<KeepBallTalentBadge> badges = team == Team.Blue ? blueTalentBadges : redTalentBadges;
            RectTransform panel = team == Team.Blue ? blueTalentPanel : redTalentPanel;
            int[] counts = GetTalentCounts(team);
            int visibleIndex = 0;

            for (int i = 0; i < TalentTypeCount; i++)
            {
                if (counts[i] <= 0)
                {
                    continue;
                }

                TalentId talentId = (TalentId)i;
                KeepBallTalentBadge badge = GetTalentBadge(team, visibleIndex, panel);
                badge.gameObject.SetActive(true);
                badge.SetData(
                    squareSprite,
                    talentIconSprites[i] != null ? talentIconSprites[i] : ballSprite,
                    GetTalentName(talentId),
                    counts[i],
                    GetTalentMaxCount(talentId),
                    GetTalentColor(talentId),
                    team);

                RectTransform badgeRect = badge.RectTransform;
                Vector2 anchor = team == Team.Blue ? new Vector2(0f, 0f) : new Vector2(1f, 0f);
                badgeRect.anchorMin = anchor;
                badgeRect.anchorMax = anchor;
                badgeRect.pivot = anchor;
                badgeRect.sizeDelta = new Vector2(TalentBadgeWidth, TalentBadgeHeight);
                badgeRect.anchoredPosition = new Vector2(0f, visibleIndex * (TalentBadgeHeight + TalentBadgeGap));
                badgeRect.SetSiblingIndex(visibleIndex);

                visibleIndex++;
            }

            for (int i = visibleIndex; i < badges.Count; i++)
            {
                if (badges[i] != null)
                {
                    badges[i].gameObject.SetActive(false);
                }
            }
        }

        private KeepBallTalentBadge GetTalentBadge(Team team, int index, RectTransform parent)
        {
            List<KeepBallTalentBadge> badges = team == Team.Blue ? blueTalentBadges : redTalentBadges;
            while (badges.Count <= index)
            {
                KeepBallTalentBadge badge;
                if (talentBadgePrefab != null)
                {
                    badge = Instantiate(talentBadgePrefab, parent);
                }
                else
                {
                    GameObject badgeObject = new GameObject("KeepBallTalentBadge", typeof(RectTransform));
                    badgeObject.transform.SetParent(parent, false);
                    badge = badgeObject.AddComponent<KeepBallTalentBadge>();
                }

                badges.Add(badge);
            }

            KeepBallTalentBadge result = badges[index];
            if (result.transform.parent != parent)
            {
                result.transform.SetParent(parent, false);
            }

            return result;
        }

        private void RestartMatch()
        {
            blueScore = 0;
            redScore = 0;
            nextKickoffTeam = Team.Blue;
            talentSelectionOpen = false;
            lastBlueTalentText = "暂无";
            lastRedTalentText = "暂无";
            ClearExtraPlayers();
            ResetTalents();
            ResetRound();
            ShowMessage("重新开始");
        }

        private void ToggleMatchMode()
        {
            matchMode = IsPvpMode() ? MatchMode.Pve : MatchMode.Pvp;
            RestartMatch();
            ShowMessage($"切换为 {GetMatchModeLabel()}");
        }

        private void ResetRound()
        {
            RestoreGoalPresentationState();
            goalLocked = false;
            talentSelectionOpen = false;
            talentSecondPickPending = false;
            selectedTalentIndex = 0;
            nextTalentMoveInputTime = 0f;
            chargeTime = 0f;
            SetHoldChargeArmed(Team.Blue, false);
            SetHoldChargeArmed(Team.Red, false);
            bluePhantomPlayerUsesRemaining = HasTalent(Team.Blue, TalentId.PhantomPlayer) ? 1 : 0;
            redPhantomPlayerUsesRemaining = HasTalent(Team.Red, TalentId.PhantomPlayer) ? 1 : 0;
            bluePhantomPlayerPassWindowActive = false;
            redPhantomPlayerPassWindowActive = false;
            redCurrentHoldDelay = redAutoHoldDelay;
            autoControlCooldownUntil = Time.time + redKickoffAutoHoldLockout;
            DestroyActivePhantomPlayer();
            ClearPhantomBalls();
            ClearKickoffLock();

            ResetTeamToFormation(Team.Blue);
            ResetTeamToFormation(Team.Red);

            PlayerAgent kickoffPlayer = FindKickoffPlayer(nextKickoffTeam);
            if (kickoffPlayer != null)
            {
                lockedKickoffPosition = GetFixedKickoffPosition(nextKickoffTeam);
                lockedKickoffPlayer = kickoffPlayer;
                kickoffPlayerLocked = true;
                kickoffPlayer.ResetToPosition(lockedKickoffPosition);
                SyncPlayerControlRangeVisual(kickoffPlayer);

                Vector2 kickoffDirection = nextKickoffTeam == Team.Blue ? Vector2.right : Vector2.left;
                float kickoffAngularSpeed = nextKickoffTeam == Team.Blue ? holdAngularSpeed : -holdAngularSpeed;
                Vector2 startPosition = lockedKickoffPosition + kickoffDirection * minHoldRadius;
                ball.ResetBall(startPosition, Vector2.zero);
                ball.BeginHold(kickoffPlayer, minHoldRadius, kickoffAngularSpeed);

                if (nextKickoffTeam == Team.Red)
                {
                    redCurrentHoldDelay = RollRedHoldDelay();
                }

                ShowMessage($"{GetTeamLabel(nextKickoffTeam)}开球：{kickoffPlayer.name}");
            }
            else
            {
                ClearKickoffLock();
                float fallbackX = nextKickoffTeam == Team.Blue ? -fieldWidth * 0.25f : fieldWidth * 0.25f;
                Vector2 startPosition = new Vector2(fallbackX, 0f);
                ball.ResetBall(startPosition, Vector2.zero);
                ShowMessage($"{GetTeamLabel(nextKickoffTeam)}开球");
            }

            UpdateControlHighlights();
        }

        private void TryCatchBall(Team team)
        {
            PlayerAgent catcher = FindNearestCatchablePlayer(team);
            if (catcher == null)
            {
                ShowMessage($"没有{GetTeamLabel(team)}在抓球范围内");
                return;
            }

            chargeTime = 0f;
            SetHoldChargeArmed(Team.Blue, false);
            SetHoldChargeArmed(Team.Red, false);
            SetHoldChargeArmed(team, true);
            SetPhantomPlayerPassWindowActive(Team.Blue, false);
            SetPhantomPlayerPassWindowActive(Team.Red, false);
            ClearKickoffLockIfHolder(ball.Holder);
            DestroyActivePhantomPlayer();
            ball.BeginHold(catcher, minHoldRadius, team == Team.Blue ? holdAngularSpeed : -holdAngularSpeed);
            ApplyCatchTalents(catcher.Team, catcher.Position);
            ShowMessage($"抓住：{catcher.name}");
        }

        private bool TryAutoRedCatchBall()
        {
            PlayerAgent redCatcher = FindNearestControllingPlayer(redPlayers);
            if (redCatcher == null)
            {
                return false;
            }

            chargeTime = 0f;
            SetHoldChargeArmed(Team.Blue, false);
            SetHoldChargeArmed(Team.Red, false);
            SetPhantomPlayerPassWindowActive(Team.Blue, false);
            SetPhantomPlayerPassWindowActive(Team.Red, false);
            ClearKickoffLockIfHolder(ball.Holder);
            DestroyActivePhantomPlayer();
            redCurrentHoldDelay = RollRedHoldDelay();
            ball.BeginHold(redCatcher, minHoldRadius, -holdAngularSpeed);
            ApplyCatchTalents(redCatcher.Team, redCatcher.Position);
            ShowMessage($"红方拿球：{redCatcher.name}");
            return true;
        }

        private void ReleasePlayerHeldBall()
        {
            PlayerAgent holder = ball.Holder;
            Team releaseTeam = holder != null ? holder.Team : Team.Blue;
            float charge01 = Mathf.Clamp01(chargeTime / maxChargeTime);
            float releaseSpeed = Mathf.Lerp(minReleaseSpeed, maxReleaseSpeed, charge01);
            Vector2 releaseDirection = ball.Release(releaseSpeed);
            ClearKickoffLockIfHolder(holder);
            ApplyReleaseTalents(releaseTeam, ball.Position, releaseDirection, releaseSpeed);
            SetPhantomPlayerPassWindowActive(Team.Blue, false);
            SetPhantomPlayerPassWindowActive(Team.Red, false);
            if (GetPhantomPlayerUsesRemaining(releaseTeam) > 0 && HasTalent(releaseTeam, TalentId.PhantomPlayer))
            {
                SetPhantomPlayerPassWindowActive(releaseTeam, true);
            }

            ShowMessage(charge01 >= 0.95f ? "爆射！" : "释放！");
            chargeTime = 0f;
            SetHoldChargeArmed(Team.Blue, false);
            SetHoldChargeArmed(Team.Red, false);
            autoControlCooldownUntil = Time.time + 0.35f;
        }

        private void ReleaseRedHeldBall()
        {
            PlayerAgent holder = ball.Holder;
            Team releaseTeam = holder != null ? holder.Team : Team.Red;
            Vector2 target = GetRedReleaseTarget(out float releaseSpeed, out string message);
            float finalSpeed = ApplyRedKickSpeedVariance(releaseSpeed);
            Vector2 releaseDirection = ball.ReleaseToward(target, finalSpeed);
            ClearKickoffLockIfHolder(holder);
            ApplyReleaseTalents(releaseTeam, ball.Position, releaseDirection, finalSpeed);
            SetPhantomPlayerPassWindowActive(Team.Blue, false);
            SetPhantomPlayerPassWindowActive(Team.Red, false);
            ShowMessage(message);
            chargeTime = 0f;
            SetHoldChargeArmed(Team.Blue, false);
            SetHoldChargeArmed(Team.Red, false);
            autoControlCooldownUntil = Time.time + 0.45f;
        }

        private bool TryActivatePhantomPlayer(Team team)
        {
            if (GetPhantomPlayerUsesRemaining(team) <= 0 || !IsPhantomPlayerPassWindowActive(team) || activePhantomPlayer != null)
            {
                return false;
            }

            if (!HasTalent(team, TalentId.PhantomPlayer) || ball.State != BallState.Free)
            {
                return false;
            }

            PlayerAgent prefab = phantomPlayerPrefab != null ? phantomPlayerPrefab : team == Team.Blue ? bluePlayerPrefab : redPlayerPrefab;
            if (prefab == null)
            {
                ShowMessage("缺少幻影球员预制体");
                return true;
            }

            SetPhantomPlayerUsesRemaining(team, 0);
            SetPhantomPlayerPassWindowActive(team, false);

            Vector2 spawnPosition = ClampInsideField(ball.Position);
            activePhantomPlayer = Instantiate(prefab, spawnPosition, Quaternion.identity, playersRoot);
            activePhantomPlayer.Initialize(
                team,
                PlayerRole.Midfielder,
                0,
                spawnPosition,
                team == Team.Blue ? bluePlayerSprite : redPlayerSprite,
                controlRangeSprite,
                holdPointSprite,
                playerRadius * 0.82f,
                catchRadius,
                holdPointRadius);
            Color phantomColor = GetPhantomPlayerColor(team);
            activePhantomPlayer.OverrideVisualColors(phantomColor, phantomColor);

            activePhantomPlayerTeam = team;
            ball.BeginHold(activePhantomPlayer, minHoldRadius, team == Team.Blue ? phantomPlayerHoldAngularSpeed : -phantomPlayerHoldAngularSpeed);
            chargeTime = 0f;
            SetHoldChargeArmed(Team.Blue, false);
            SetHoldChargeArmed(Team.Red, false);
            autoControlCooldownUntil = Time.time + 0.35f;
            ShowMessage($"{GetTeamLabel(team)}幻影球员接管足球");
            return true;
        }

        private void ReleasePhantomPlayerBall()
        {
            if (activePhantomPlayer == null || ball.Holder != activePhantomPlayer)
            {
                DestroyActivePhantomPlayer();
                return;
            }

            Vector2 origin = activePhantomPlayer.Position;
            Vector2 releaseDirection = ball.ReleaseFromHolderPosition(phantomPlayerKickSpeed);
            ApplyReleaseTalents(activePhantomPlayerTeam, origin, releaseDirection, phantomPlayerKickSpeed);
            DestroyActivePhantomPlayer();
            chargeTime = 0f;
            SetHoldChargeArmed(Team.Blue, false);
            SetHoldChargeArmed(Team.Red, false);
            autoControlCooldownUntil = Time.time + 0.35f;
            ShowMessage($"{GetTeamLabel(activePhantomPlayerTeam)}幻影球员踢出足球");
        }

        private PlayerAgent FindKickoffPlayer(Team team)
        {
            List<PlayerAgent> players = team == Team.Blue ? bluePlayers : redPlayers;
            PlayerAgent defender = FindLastRole(players, PlayerRole.Defender);
            if (defender != null)
            {
                return defender;
            }

            return players.Count > 0 ? players[0] : null;
        }

        private Vector2 GetFixedKickoffPosition(Team team)
        {
            float side = team == Team.Blue ? -1f : 1f;
            return ClampInsideField(new Vector2(side * fieldWidth * GetRoleFormationRatio(PlayerRole.Defender), 0f));
        }

        private void ClearKickoffLockIfHolder(PlayerAgent holder)
        {
            if (holder != null && holder == lockedKickoffPlayer)
            {
                ClearKickoffLock();
            }
        }

        private void ClearKickoffLock()
        {
            kickoffPlayerLocked = false;
            lockedKickoffPlayer = null;
            lockedKickoffPosition = Vector2.zero;
        }

        private PlayerAgent FindLastRole(List<PlayerAgent> players, PlayerRole role)
        {
            for (int i = players.Count - 1; i >= 0; i--)
            {
                PlayerAgent player = players[i];
                if (player.Role == role)
                {
                    return player;
                }
            }

            return null;
        }

        private Vector2 GetRedReleaseTarget(out float releaseSpeed, out string message)
        {
            PlayerAgent holder = ball.Holder;
            releaseSpeed = redPassSpeed;

            if (holder == null)
            {
                message = "红方传出";
                return ApplyRedShotAimError(GetLeftGoalShotTarget());
            }

            if (ShouldRedMakeBadDecision())
            {
                return GetRedMistakeTarget(holder, out releaseSpeed, out message);
            }

            if (holder.Role == PlayerRole.Defender && TryFindRedPassTarget(PlayerRole.Midfielder, holder, out PlayerAgent midfielder))
            {
                message = $"红方后卫传中场：{midfielder.name}";
                return ApplyRedPassAimError(midfielder.Position);
            }

            if (holder.Role == PlayerRole.Midfielder && TryFindRedPassTarget(PlayerRole.Forward, holder, out PlayerAgent forward))
            {
                message = $"红方中场传前锋：{forward.name}";
                return ApplyRedPassAimError(forward.Position);
            }

            if (holder.Role == PlayerRole.Defender && TryFindRedPassTarget(PlayerRole.Forward, holder, out PlayerAgent fallbackForward))
            {
                message = $"红方后卫长传前锋：{fallbackForward.name}";
                releaseSpeed = redReleaseSpeed;
                return ApplyRedPassAimError(fallbackForward.Position);
            }

            releaseSpeed = redReleaseSpeed;
            message = holder.Role == PlayerRole.Forward ? "红方前锋射门" : "红方直接射门";
            return ApplyRedShotAimError(GetLeftGoalShotTarget());
        }

        private Vector2 GetRedMistakeTarget(PlayerAgent holder, out float releaseSpeed, out string message)
        {
            if (Random.value < 0.55f && TryFindRandomRedPassTarget(holder, out PlayerAgent target))
            {
                releaseSpeed = redPassSpeed;
                message = $"红方仓促传球：{target.name}";
                return ApplyRedPassAimError(target.Position);
            }

            releaseSpeed = redReleaseSpeed;
            message = "红方尝试射门";
            return ApplyRedShotAimError(GetLeftGoalShotTarget());
        }

        private bool TryFindRandomRedPassTarget(PlayerAgent holder, out PlayerAgent target)
        {
            target = null;
            int candidateCount = 0;

            foreach (PlayerAgent player in redPlayers)
            {
                if (player == holder)
                {
                    continue;
                }

                candidateCount++;
                if (Random.Range(0, candidateCount) == 0)
                {
                    target = player;
                }
            }

            return target != null;
        }

        private bool TryFindRedPassTarget(PlayerRole role, PlayerAgent holder, out PlayerAgent target)
        {
            target = null;
            float bestScore = float.MaxValue;

            foreach (PlayerAgent player in redPlayers)
            {
                if (player == holder || player.Role != role)
                {
                    continue;
                }

                float distance = Vector2.Distance(holder.Position, player.Position);
                float forwardBonus = Mathf.Max(0f, holder.Position.x - player.Position.x) * 0.4f;
                float laneCost = Mathf.Abs(holder.Position.y - player.Position.y) * 0.25f;
                float score = distance + laneCost - forwardBonus;

                if (score < bestScore)
                {
                    bestScore = score;
                    target = player;
                }
            }

            return target != null;
        }

        private Vector2 GetLeftGoalShotTarget()
        {
            return new Vector2(-fieldWidth * 0.5f - goalDepth, Random.Range(-goalHeight * 0.25f, goalHeight * 0.25f));
        }

        private float RollRedHoldDelay()
        {
            float weakness = GetRedWeakness(redDecisionSkill);
            float baseDelay = redAutoHoldDelay + redMaxReactionDelayPenalty * weakness;
            float jitter = redMaxHoldDelayJitter * weakness;
            return Mathf.Max(0.05f, baseDelay + Random.Range(0f, jitter));
        }

        private bool ShouldRedMakeBadDecision()
        {
            return Random.value < redMaxDecisionMistakeChance * GetRedWeakness(redDecisionSkill);
        }

        private Vector2 ApplyRedPassAimError(Vector2 target)
        {
            return ApplyRedAimError(target, redMaxPassAimError, redPassSkill);
        }

        private Vector2 ApplyRedShotAimError(Vector2 target)
        {
            return ApplyRedAimError(target, redMaxShotAimError, redShotSkill);
        }

        private Vector2 ApplyRedAimError(Vector2 target, float maxError, float skill)
        {
            float errorRadius = Mathf.Max(0f, maxError) * GetRedWeakness(skill);
            return target + Random.insideUnitCircle * errorRadius;
        }

        private float ApplyRedKickSpeedVariance(float speed)
        {
            float variance = redMaxKickSpeedRandomness * GetRedWeakness(redKickPowerSkill);
            return speed * Random.Range(1f - variance, 1f + variance);
        }

        private bool CanRedAutoControlBall(PlayerAgent player)
        {
            float skill = GetRedDimensionSkill(redAutoCatchSkill);
            float catchRadiusMultiplier = Mathf.Lerp(1f - redMaxCatchRadiusPenalty, 1f, skill);
            float controlDistance = GetEffectiveCatchRadius(player) * catchRadiusMultiplier + ball.Radius;
            return Vector2.Distance(player.Position, ball.Position) <= controlDistance;
        }

        private bool CanPlayerCatchBall(PlayerAgent player, BallController targetBall)
        {
            if (targetBall == null || targetBall.IsPhantom)
            {
                return false;
            }

            float controlDistance = GetEffectiveCatchRadius(player) + targetBall.Radius;
            return Vector2.Distance(player.Position, targetBall.Position) <= controlDistance;
        }

        private bool CanPlayerInterceptPhantom(PlayerAgent player, BallController targetBall)
        {
            if (targetBall == null || !targetBall.IsPhantom || player.Team == targetBall.PhantomOwner)
            {
                return false;
            }

            float hitDistance = player.BodyRadius + targetBall.Radius;
            return Vector2.Distance(player.Position, targetBall.Position) <= hitDistance;
        }

        private float GetEffectiveCatchRadius(PlayerAgent player)
        {
            float multiplier = 1f;
            int bigfootCount = GetTalentCount(player.Team, TalentId.BigfootForward);
            if (player.Role == PlayerRole.Forward && bigfootCount > 0)
            {
                multiplier += bigfootForwardCatchBonusPerStack * bigfootCount;
            }

            return player.CatchRadius * multiplier;
        }

        private float GetRedDimensionSkill(float skill)
        {
            return Mathf.Clamp01(redAiStrength * skill);
        }

        private float GetRedWeakness(float skill)
        {
            return 1f - GetRedDimensionSkill(skill);
        }

        private PlayerAgent FindNearestCatchablePlayer(Team team)
        {
            PlayerAgent nearest = null;
            float nearestDistance = float.MaxValue;
            List<PlayerAgent> players = team == Team.Blue ? bluePlayers : redPlayers;

            foreach (PlayerAgent player in players)
            {
                float distance = Vector2.Distance(player.Position, ball.Position);
                if (CanPlayerCatchBall(player, ball) && distance < nearestDistance)
                {
                    nearest = player;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private PlayerAgent FindNearestControllingPlayer(List<PlayerAgent> players)
        {
            if (Time.time < autoControlCooldownUntil)
            {
                return null;
            }

            PlayerAgent nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (PlayerAgent player in players)
            {
                float distance = Vector2.Distance(player.Position, ball.Position);
                bool canControl = player.Team == Team.Red ? CanRedAutoControlBall(player) : player.CanControlBall(ball);
                if (canControl && distance < nearestDistance)
                {
                    nearest = player;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private void UpdateControlHighlights()
        {
            foreach (PlayerAgent player in bluePlayers)
            {
                player.SetCatchHighlighted(ball.Holder == player);
            }

            foreach (PlayerAgent player in redPlayers)
            {
                player.SetCatchHighlighted(ball.Holder == player);
            }

            if (activePhantomPlayer != null)
            {
                activePhantomPlayer.SetCatchHighlighted(ball.Holder == activePhantomPlayer);
            }
        }

        private void UpdateHoldPointRotation(float deltaTime)
        {
            foreach (PlayerAgent player in bluePlayers)
            {
                player.TickHoldPoint(deltaTime, holdAngularSpeed);
            }

            foreach (PlayerAgent player in redPlayers)
            {
                player.TickHoldPoint(deltaTime, -holdAngularSpeed);
            }
        }

        private bool TryStealHeldBall()
        {
            if (Time.time < autoControlCooldownUntil || ball.Holder == null)
            {
                return false;
            }

            List<PlayerAgent> challengers = ball.Holder.Team == Team.Blue ? redPlayers : bluePlayers;
            PlayerAgent stealer = FindNearestHoldPointStealer(challengers);
            if (stealer == null || stealer == ball.Holder)
            {
                return false;
            }

            chargeTime = 0f;
            if (stealer.Team == Team.Red)
            {
                redCurrentHoldDelay = RollRedHoldDelay();
            }

            ClearKickoffLockIfHolder(ball.Holder);
            ball.BeginHold(stealer, minHoldRadius, stealer.Team == Team.Red ? -holdAngularSpeed : holdAngularSpeed);
            autoControlCooldownUntil = Time.time + 0.2f;
            ShowMessage($"{(stealer.Team == Team.Blue ? "蓝方" : "红方")}抢断");
            return true;
        }

        private PlayerAgent FindNearestHoldPointStealer(List<PlayerAgent> players)
        {
            PlayerAgent nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (PlayerAgent player in players)
            {
                float distance = Vector2.Distance(player.HoldPointPosition, ball.Position);
                if (player.IsHoldPointTouchingBall(ball) && distance < nearestDistance)
                {
                    nearest = player;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private void UpdatePlayerMovement(float deltaTime)
        {
            for (int i = 0; i < bluePlayers.Count; i++)
            {
                PlayerAgent player = bluePlayers[i];
                Vector2 target = GetBlueSupportTarget(player, i);
                MovePlayer(player, target, bluePlayers, blueMoveSpeed, deltaTime);
            }

            for (int i = 0; i < redPlayers.Count; i++)
            {
                PlayerAgent player = redPlayers[i];
                Vector2 target = GetRedPressureTarget(player, i);
                float moveSpeed = IsPvpMode() ? redMoveSpeed : redMoveSpeed * Mathf.Lerp(0.65f, 1f, GetRedDimensionSkill(redMovementSkill));
                MovePlayer(player, target, redPlayers, moveSpeed, deltaTime);
            }
        }

        private Vector2 GetBlueSupportTarget(PlayerAgent player, int index)
        {
            return GetRoleTarget(player, index, Team.Blue);
        }

        private Vector2 GetRedPressureTarget(PlayerAgent player, int index)
        {
            PlayerAgent holder = ball.Holder;

            if (ball.State == BallState.Held && holder != null && holder.Team == Team.Blue)
            {
                int holderRank = GetDistanceRank(player, redPlayers, holder.Position);
                if (holderRank < 2)
                {
                    Vector2 sideOffset = new Vector2(0f, holderRank == 0 ? -0.7f : 0.7f);
                    return ClampInsideField(holder.Position + sideOffset);
                }
            }

            return GetRoleTarget(player, index, Team.Red);
        }

        private Vector2 GetRoleTarget(PlayerAgent player, int index, Team team)
        {
            float side = team == Team.Blue ? -1f : 1f;
            float attackDirection = team == Team.Blue ? 1f : -1f;

            switch (player.Role)
            {
                case PlayerRole.Forward:
                    return GetForwardTarget(player, team, attackDirection);
                case PlayerRole.Defender:
                    return GetDefenderTarget(player, team, side);
                default:
                    return GetMidfielderTarget(player, index, team, side, attackDirection);
            }
        }

        private Vector2 GetForwardTarget(PlayerAgent player, Team team, float attackDirection)
        {
            float lane = GetRoleLaneForPlayer(player, team, fieldHeight * 0.31f);
            float advancedX = attackDirection * fieldWidth * 0.28f;
            float ballAheadX = ball.Position.x + attackDirection * 4.2f;
            float x = attackDirection > 0f ? Mathf.Max(advancedX, ballAheadX) : Mathf.Min(advancedX, ballAheadX);
            x = Mathf.Clamp(x, -fieldWidth * 0.43f, fieldWidth * 0.43f);

            float y = Mathf.Lerp(lane, ball.Position.y * 0.45f + lane * 0.55f, 0.45f);
            return ClampInsideField(new Vector2(x, y));
        }

        private Vector2 GetDefenderTarget(PlayerAgent player, Team team, float side)
        {
            float lane = GetRoleLaneForPlayer(player, team, fieldHeight * 0.28f);
            float baseX = side * fieldWidth * 0.34f;
            float ballInOwnHalf = side < 0f ? Mathf.Clamp01(-ball.Position.x / (fieldWidth * 0.5f)) : Mathf.Clamp01(ball.Position.x / (fieldWidth * 0.5f));
            float x = Mathf.Lerp(baseX, ball.Position.x, ballInOwnHalf * 0.35f);
            float ownHalfLimit = side < 0f ? -0.8f : 0.8f;
            x = side < 0f ? Mathf.Min(x, ownHalfLimit) : Mathf.Max(x, ownHalfLimit);

            float y = Mathf.Lerp(lane, ball.Position.y, 0.35f);
            return ClampInsideField(new Vector2(x, y));
        }

        private Vector2 GetMidfielderTarget(PlayerAgent player, int index, Team team, float side, float attackDirection)
        {
            Vector2 ballPosition = ball.Position;
            int ballRank = GetDistanceRank(player, team == Team.Blue ? bluePlayers : redPlayers, ballPosition);
            float lane = GetRoleLaneForPlayer(player, team, fieldHeight * 0.27f);
            float sideOffset = ballRank % 2 == 0 ? -1.35f : 1.35f;
            float xOffset = attackDirection * (ballRank == 0 ? -0.7f : 0.9f);
            Vector2 nearBallTarget = ballPosition + new Vector2(xOffset, sideOffset);

            float midfieldX = side * fieldWidth * 0.06f;
            Vector2 shapeTarget = new Vector2(midfieldX, lane);
            return ClampInsideField(Vector2.Lerp(shapeTarget, nearBallTarget, 0.72f));
        }

        private void MovePlayer(PlayerAgent player, Vector2 target, List<PlayerAgent> teamPlayers, float speed, float deltaTime)
        {
            if (kickoffPlayerLocked && player == lockedKickoffPlayer)
            {
                player.MoveTo(lockedKickoffPosition);
                return;
            }

            if (player.IsKnockbackActive)
            {
                return;
            }

            speed *= GetRoleSpeedMultiplier(player.Role);

            if (ball.State == BallState.Held && ball.Holder == player)
            {
                speed *= holderMoveSpeedMultiplier;
            }

            Vector2 current = player.Position;
            Vector2 separation = GetSeparation(player, teamPlayers);
            Vector2 adjustedTarget = target + separation * separationStrength;
            Vector2 next = Vector2.MoveTowards(current, adjustedTarget, speed * deltaTime);
            player.MoveTo(ClampInsideField(next));
        }

        private float GetRoleSpeedMultiplier(PlayerRole role)
        {
            switch (role)
            {
                case PlayerRole.Forward:
                    return forwardMoveSpeedMultiplier;
                case PlayerRole.Defender:
                    return defenderMoveSpeedMultiplier;
                default:
                    return midfielderMoveSpeedMultiplier;
            }
        }

        private float GetRoleLaneForIndex(int index, int total, PlayerRole role, float halfRange)
        {
            int roleCount = 0;
            int roleSlot = 0;

            for (int i = 0; i < total; i++)
            {
                if (GetRoleForIndex(i, total) != role)
                {
                    continue;
                }

                if (i < index)
                {
                    roleSlot++;
                }

                roleCount++;
            }

            float t = roleCount <= 1 ? 0.5f : roleSlot / (float)(roleCount - 1);
            return Mathf.Lerp(-halfRange, halfRange, t);
        }

        private float GetRoleLaneForPlayer(PlayerAgent player, Team team, float halfRange)
        {
            List<PlayerAgent> players = team == Team.Blue ? bluePlayers : redPlayers;
            int roleSlot = GetRoleSlot(players, player);
            int roleCount = CountRole(players, player.Role);
            return GetLaneForRoleSlot(roleSlot, roleCount, halfRange);
        }

        private float GetLaneForRoleSlot(int roleSlot, int roleCount, float halfRange)
        {
            float t = roleCount <= 1 ? 0.5f : roleSlot / (float)(roleCount - 1);
            return Mathf.Lerp(-halfRange, halfRange, Mathf.Clamp01(t));
        }

        private int GetRoleSlot(List<PlayerAgent> players, PlayerAgent player)
        {
            int roleSlot = 0;
            foreach (PlayerAgent other in players)
            {
                if (other == player)
                {
                    return roleSlot;
                }

                if (other.Role == player.Role)
                {
                    roleSlot++;
                }
            }

            return roleSlot;
        }

        private int CountRole(List<PlayerAgent> players, PlayerRole role)
        {
            int count = 0;
            foreach (PlayerAgent player in players)
            {
                if (player.Role == role)
                {
                    count++;
                }
            }

            return count;
        }

        private Vector2 GetSeparation(PlayerAgent player, List<PlayerAgent> teamPlayers)
        {
            Vector2 push = Vector2.zero;

            foreach (PlayerAgent other in teamPlayers)
            {
                if (other == player)
                {
                    continue;
                }

                Vector2 difference = player.Position - other.Position;
                float distance = difference.magnitude;
                if (distance > 0.001f && distance < separationRadius)
                {
                    push += difference.normalized * (1f - distance / separationRadius);
                }
            }

            return push;
        }

        private int GetDistanceRank(PlayerAgent player, List<PlayerAgent> players, Vector2 target)
        {
            int rank = 0;
            float distance = Vector2.SqrMagnitude(player.Position - target);

            foreach (PlayerAgent other in players)
            {
                if (other != player && Vector2.SqrMagnitude(other.Position - target) < distance)
                {
                    rank++;
                }
            }

            return rank;
        }

        private Vector2 ClampInsideField(Vector2 position)
        {
            float margin = playerRadius + 0.25f;
            float halfWidth = fieldWidth * 0.5f - margin;
            float halfHeight = fieldHeight * 0.5f - margin;
            return new Vector2(
                Mathf.Clamp(position.x, -halfWidth, halfWidth),
                Mathf.Clamp(position.y, -halfHeight, halfHeight));
        }

        public void OnGoal(GoalSide side, BallController scoringBall = null)
        {
            if (goalLocked)
            {
                return;
            }

            if (IsPhantomOwnGoal(side, scoringBall))
            {
                Team ownGoalOwner = scoringBall.PhantomOwner;
                DestroyPhantomBall(scoringBall);
                ShowMessage($"{GetTeamLabel(ownGoalOwner)}幻影足球进入自家球门，不计分");
                return;
            }

            goalLocked = true;
            Vector2 goalFocusPosition = scoringBall != null ? scoringBall.Position : GetGoalFocusFallback(side);
            if (scoringBall != null)
            {
                scoringBall.FreezeForGoalPresentation(goalFocusPosition);
            }

            string goalMessage;
            Color goalColor;

            if (side == GoalSide.Right)
            {
                blueScore++;
                nextKickoffTeam = Team.Red;
                talentFirstPickTeam = Team.Blue;
                talentSecondPickTeam = Team.Red;
                goalMessage = "蓝方进球！";
                goalColor = new Color(0.35f, 0.62f, 1f, 1f);
            }
            else
            {
                redScore++;
                nextKickoffTeam = Team.Blue;
                talentFirstPickTeam = Team.Red;
                talentSecondPickTeam = Team.Blue;
                goalMessage = "红方进球！";
                goalColor = new Color(1f, 0.32f, 0.28f, 1f);
            }

            bluePhantomPlayerUsesRemaining = 0;
            redPhantomPlayerUsesRemaining = 0;
            bluePhantomPlayerPassWindowActive = false;
            redPhantomPlayerPassWindowActive = false;
            ClearKickoffLock();
            DestroyActivePhantomPlayer();

            SpawnGoalEffect(goalFocusPosition);
            StartCoroutine(PlayGoalSequence(goalFocusPosition, goalMessage, goalColor));
        }

        private Vector2 GetGoalFocusFallback(GoalSide side)
        {
            float x = side == GoalSide.Right ? fieldWidth * 0.5f + goalDepth * 0.5f : -fieldWidth * 0.5f - goalDepth * 0.5f;
            return new Vector2(x, 0f);
        }

        private bool IsPhantomOwnGoal(GoalSide side, BallController scoringBall)
        {
            if (scoringBall == null || !scoringBall.IsPhantom)
            {
                return false;
            }

            return (scoringBall.PhantomOwner == Team.Blue && side == GoalSide.Left) ||
                (scoringBall.PhantomOwner == Team.Red && side == GoalSide.Right);
        }

        private void DestroyPhantomBall(BallController phantom)
        {
            if (phantom == null)
            {
                return;
            }

            phantomBalls.Remove(phantom);
            Destroy(phantom.gameObject);
        }

        private void SpawnGoalEffect(Vector2 position)
        {
            if (goalEffectPrefab == null)
            {
                return;
            }

            GameObject effect = Instantiate(goalEffectPrefab, new Vector3(position.x, position.y, 0f), Quaternion.identity, transform);
            ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < particleSystems.Length; i++)
            {
                particleSystems[i].Play(true);
            }

            StartCoroutine(DestroyGoalEffectAfterLifetime(effect));
        }

        private IEnumerator DestroyGoalEffectAfterLifetime(GameObject effect)
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, goalEffectLifetime));

            if (effect != null)
            {
                Destroy(effect);
            }
        }

        private IEnumerator PlayGoalSequence(Vector2 focusPosition, string goalMessage, Color goalColor)
        {
            Camera cameraToAnimate = gameplayCamera != null ? gameplayCamera : Camera.main;
            Vector3 startPosition = cameraToAnimate != null ? cameraToAnimate.transform.position : defaultCameraPosition;
            float startSize = cameraToAnimate != null ? cameraToAnimate.orthographicSize : defaultCameraOrthographicSize;
            Vector3 focusCameraPosition = new Vector3(focusPosition.x, focusPosition.y, defaultCameraPosition.z);

            Time.timeScale = Mathf.Clamp(goalSlowTimeScale, 0.05f, 1f);
            Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;

            if (cameraToAnimate != null)
            {
                yield return AnimateCamera(cameraToAnimate, startPosition, focusCameraPosition, startSize, goalZoomOrthographicSize, goalCameraMoveDuration);
            }

            goalBannerText = goalMessage;
            goalBannerColor = goalColor;
            goalBannerUntilRealtime = Time.unscaledTime + goalDisplayDuration;
            ShowMessage(goalMessage);

            yield return new WaitForSecondsRealtime(goalDisplayDuration);

            RestoreGoalTimeScale();

            if (cameraToAnimate != null)
            {
                yield return AnimateCamera(cameraToAnimate, cameraToAnimate.transform.position, defaultCameraPosition, cameraToAnimate.orthographicSize, defaultCameraOrthographicSize, goalCameraReturnDuration);
            }

            goalBannerText = string.Empty;
            ShowTalentSelectionAfterGoalPresentation();
        }

        private IEnumerator AnimateCamera(Camera targetCamera, Vector3 fromPosition, Vector3 toPosition, float fromSize, float toSize, float duration)
        {
            float safeDuration = Mathf.Max(0.01f, duration);
            float elapsed = 0f;

            while (elapsed < safeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / safeDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                targetCamera.transform.position = Vector3.Lerp(fromPosition, toPosition, eased);
                targetCamera.orthographicSize = Mathf.Lerp(fromSize, toSize, eased);
                yield return null;
            }

            targetCamera.transform.position = toPosition;
            targetCamera.orthographicSize = toSize;
        }

        private void RestoreGoalPresentationState()
        {
            RestoreGoalTimeScale();
            goalBannerText = string.Empty;

            Camera cameraToRestore = gameplayCamera != null ? gameplayCamera : Camera.main;
            if (cameraToRestore == null)
            {
                return;
            }

            cameraToRestore.transform.position = defaultCameraPosition;
            cameraToRestore.orthographicSize = defaultCameraOrthographicSize;
        }

        private void RestoreGoalTimeScale()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = defaultFixedDeltaTime > 0f ? defaultFixedDeltaTime : 0.02f;
        }

        private void ShowTalentSelectionAfterGoalPresentation()
        {
            currentTalentPickingTeam = talentFirstPickTeam;
            talentSecondPickPending = false;
            GenerateTalentChoices();
            talentSelectionOpen = true;
            ShowMessage($"{GetTeamLabel(currentTalentPickingTeam)}先选择天赋");
        }

        private IEnumerator ShowTalentSelectionAfterGoal()
        {
            yield return new WaitForSeconds(0.8f);
            ShowTalentSelectionAfterGoalPresentation();
        }

        private void GenerateTalentChoices()
        {
            selectedTalentIndex = 0;
            nextTalentMoveInputTime = 0f;
            for (int i = 0; i < talentOptionTaken.Length; i++)
            {
                talentOptionTaken[i] = false;
            }

            TalentId[] pool =
            {
                TalentId.PhantomFootball,
                TalentId.BigfootForward,
                TalentId.ExtraForward,
                TalentId.ExtraMidfielder,
                TalentId.ExtraDefender,
                TalentId.ShieldField,
                TalentId.PassField,
                TalentId.PhantomPlayer
            };
            List<TalentId> availablePool = new List<TalentId>();
            foreach (TalentId talentId in pool)
            {
                if (CanGainTalent(currentTalentPickingTeam, talentId))
                {
                    availablePool.Add(talentId);
                }
            }
            List<TalentId> fallbackAvailablePool = new List<TalentId>(availablePool);

            for (int i = 0; i < TalentChoiceCount; i++)
            {
                if (availablePool.Count == 0)
                {
                    availablePool = fallbackAvailablePool.Count > 0 ? new List<TalentId>(fallbackAvailablePool) : new List<TalentId>(pool);
                }

                int pickIndex = Random.Range(0, availablePool.Count);
                TalentId picked = availablePool[pickIndex];
                availablePool.RemoveAt(pickIndex);
                currentTalentOptions[i] = CreateTalentOption(picked);
            }

            selectedTalentIndex = FindFirstSelectableTalentIndex(currentTalentPickingTeam);
            if (selectedTalentIndex < 0)
            {
                selectedTalentIndex = 0;
            }
        }

        private TalentOption CreateTalentOption(TalentId id)
        {
            switch (id)
            {
                case TalentId.PhantomFootball:
                    return new TalentOption
                    {
                        Id = id,
                        Name = "幻影足球",
                        Description = "每层传球时多生成一颗幻影足球，最多 3 层。多颗幻影足球会平分 60 度散射角。幻影足球弹射 3 次或被敌人拦截后消失，进球同样有效。",
                        AccentColor = new Color(0.42f, 0.72f, 1f, 1f)
                    };
                case TalentId.BigfootForward:
                    return new TalentOption
                    {
                        Id = id,
                        Name = "大脚怪前锋",
                        Description = "前锋接球范围每层提高 10%，最多 3 层。",
                        AccentColor = new Color(1f, 0.6f, 0.18f, 1f)
                    };
                case TalentId.ExtraForward:
                    return new TalentOption
                    {
                        Id = id,
                        Name = "额外前锋",
                        Description = "额外获得一个右方前锋球员，最多 2 层。",
                        AccentColor = new Color(1f, 0.28f, 0.22f, 1f)
                    };
                case TalentId.ExtraMidfielder:
                    return new TalentOption
                    {
                        Id = id,
                        Name = "额外中场",
                        Description = "额外获得一个右方中场球员，最多 2 层。",
                        AccentColor = new Color(0.38f, 1f, 0.44f, 1f)
                    };
                case TalentId.ExtraDefender:
                    return new TalentOption
                    {
                        Id = id,
                        Name = "额外后卫",
                        Description = "额外获得一个右方后卫球员，最多 2 层。",
                        AccentColor = new Color(0.35f, 0.58f, 1f, 1f)
                    };
                case TalentId.ShieldField:
                    return new TalentOption
                    {
                        Id = id,
                        Name = "护球立场",
                        Description = "己方接球时产生圆形震荡波，推开周围敌方球员。最多 3 层，击退效果保持当前叠加方式。",
                        AccentColor = new Color(0.25f, 1f, 0.86f, 1f)
                    };
                case TalentId.PassField:
                    return new TalentOption
                    {
                        Id = id,
                        Name = "传球立场",
                        Description = "己方传球时产生圆形震荡波，推开周围敌方球员。最多 3 层，击退效果保持当前叠加方式。",
                        AccentColor = new Color(0.78f, 0.45f, 1f, 1f)
                    };
                case TalentId.PhantomPlayer:
                    return new TalentOption
                    {
                        Id = id,
                        Name = "幻影球员",
                        Description = "己方传球后可按 Z/B 在球所在位置生成幻影球员，球会绕着幻影球员运动。每次进球后的新一轮最多使用 1 次。",
                        AccentColor = new Color(0.42f, 0.92f, 1f, 1f)
                    };
                default:
                    return new TalentOption
                    {
                        Id = id,
                        Name = "香蕉球",
                        Description = "你的传球会变成带弧线的轨迹，更容易绕开防守。",
                        AccentColor = new Color(1f, 0.9f, 0.2f, 1f)
                    };
            }
        }

        private void SelectTalent(int index)
        {
            if (!talentSelectionOpen || index < 0 || index >= currentTalentOptions.Length)
            {
                return;
            }

            if (!CanSelectTalentOption(index, currentTalentPickingTeam))
            {
                selectedTalentIndex = FindFirstSelectableTalentIndex(currentTalentPickingTeam);
                ShowMessage("这个天赋已被选择或已满");
                return;
            }

            TalentOption pickedTalent = currentTalentOptions[index];
            bool gained = ApplyTalent(currentTalentPickingTeam, pickedTalent);
            string gainedText = gained ? pickedTalent.Name : $"{pickedTalent.Name}已满";
            if (currentTalentPickingTeam == Team.Blue)
            {
                lastBlueTalentText = gainedText;
            }
            else
            {
                lastRedTalentText = gainedText;
            }

            talentOptionTaken[index] = true;

            if (!talentSecondPickPending)
            {
                talentSecondPickPending = true;
                currentTalentPickingTeam = talentSecondPickTeam;
                selectedTalentIndex = FindFirstSelectableTalentIndex(currentTalentPickingTeam);
                if (selectedTalentIndex < 0)
                {
                    talentSelectionOpen = false;
                    ShowMessage($"{GetTeamLabel(talentSecondPickTeam)}没有可选的剩余天赋");
                    ResetRound();
                    return;
                }

                ShowMessage($"{GetTeamLabel(talentFirstPickTeam)}获得：{pickedTalent.Name}；{GetTeamLabel(talentSecondPickTeam)}从剩余天赋中选择");
                return;
            }

            talentSelectionOpen = false;
            talentSecondPickPending = false;
            ShowMessage($"蓝方获得：{lastBlueTalentText}；红方获得：{lastRedTalentText}");
            ResetRound();
        }

        private bool CanSelectTalentOption(int index, Team team)
        {
            return index >= 0 &&
                index < currentTalentOptions.Length &&
                !talentOptionTaken[index] &&
                CanGainTalent(team, currentTalentOptions[index].Id);
        }

        private int FindFirstSelectableTalentIndex(Team team)
        {
            for (int i = 0; i < currentTalentOptions.Length; i++)
            {
                if (CanSelectTalentOption(i, team))
                {
                    return i;
                }
            }

            return -1;
        }

        private bool ApplyTalent(Team team, TalentOption talent)
        {
            if (!CanGainTalent(team, talent.Id))
            {
                return false;
            }

            int[] counts = GetTalentCounts(team);
            counts[(int)talent.Id] = Mathf.Min(counts[(int)talent.Id] + 1, GetTalentMaxCount(talent.Id));

            switch (talent.Id)
            {
                case TalentId.ExtraForward:
                    AddExtraPlayer(team, PlayerRole.Forward);
                    break;
                case TalentId.ExtraMidfielder:
                    AddExtraPlayer(team, PlayerRole.Midfielder);
                    break;
                case TalentId.ExtraDefender:
                    AddExtraPlayer(team, PlayerRole.Defender);
                    break;
            }

            SyncTeamControlRangeVisuals(team);
            UpdateTalentUi();
            return true;
        }

        private void ApplyReleaseTalents(Team team, Vector2 origin, Vector2 direction, float speed)
        {
            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            direction.Normalize();

            if (HasTalent(team, TalentId.PassField))
            {
                int passFieldCount = GetTalentCount(team, TalentId.PassField);
                TriggerShockwave(team, origin, passFieldRadius, passFieldPushDistance * Mathf.Max(1, passFieldCount), GetTalentColor(TalentId.PassField));
            }

            if (HasTalent(team, TalentId.PhantomFootball))
            {
                int phantomCount = GetTalentCount(team, TalentId.PhantomFootball);
                float halfScatterAngle = phantomBallScatterAngle * 0.5f;
                for (int i = 0; i < phantomCount; i++)
                {
                    float t = phantomCount <= 1 ? 0.5f : i / (float)(phantomCount - 1);
                    float angle = Mathf.Lerp(-halfScatterAngle, halfScatterAngle, t);
                    SpawnPhantomBall(team, origin, Rotate(direction, angle), speed * phantomBallSpeedMultiplier);
                }
            }
        }

        private void ApplyCatchTalents(Team team, Vector2 center)
        {
            if (!HasTalent(team, TalentId.ShieldField))
            {
                return;
            }

            int shieldFieldCount = GetTalentCount(team, TalentId.ShieldField);
            TriggerShockwave(team, center, shieldFieldRadius, shieldFieldPushDistance * Mathf.Max(1, shieldFieldCount), GetTalentColor(TalentId.ShieldField));
        }

        private void TriggerShockwave(Team owner, Vector2 center, float radius, float pushDistance, Color color)
        {
            List<PlayerAgent> enemies = owner == Team.Blue ? redPlayers : bluePlayers;
            foreach (PlayerAgent enemy in enemies)
            {
                Vector2 offset = enemy.Position - center;
                float distance = offset.magnitude;
                if (distance > radius)
                {
                    continue;
                }

                Vector2 direction = distance > 0.001f ? offset / distance : Random.insideUnitCircle.normalized;
                if (direction.sqrMagnitude < 0.001f)
                {
                    direction = Vector2.up;
                }

                float strength = Mathf.Lerp(pushDistance, pushDistance * 0.25f, distance / Mathf.Max(0.001f, radius));
                Vector2 targetPosition = ClampInsideField(enemy.Position + direction * strength);
                enemy.KnockbackTo(targetPosition, shockwavePushDuration);
            }

            CreateShockwaveVisual(owner, center, radius, color);
        }

        private void CreateShockwaveVisual(Team owner, Vector2 center, float radius, Color fallbackColor)
        {
            if (controlRangeSprite == null)
            {
                return;
            }

            ShockwaveEffect prefab = owner == Team.Blue ? blueShockwavePrefab : redShockwavePrefab;
            if (prefab != null)
            {
                ShockwaveEffect effect = Instantiate(prefab, center, Quaternion.identity, transform);
                effect.Initialize(controlRangeSprite, radius, shockwaveEffectDuration);
                return;
            }

            GameObject waveObject = new GameObject("TalentShockwave");
            waveObject.transform.SetParent(transform);
            waveObject.transform.position = center;
            waveObject.transform.localScale = Vector3.one * (radius * 2f);

            SpriteRenderer renderer = waveObject.AddComponent<SpriteRenderer>();
            renderer.sprite = controlRangeSprite;
            renderer.color = new Color(fallbackColor.r, fallbackColor.g, fallbackColor.b, 0.48f);
            renderer.sortingOrder = 32;

            Destroy(waveObject, shockwaveEffectDuration);
        }

        private void SpawnPhantomBall(Team owner, Vector2 origin, Vector2 direction, float speed)
        {
            BallController phantom;
            BallController prefab = phantomBallPrefab != null ? phantomBallPrefab : ballPrefab;

            if (prefab != null)
            {
                phantom = Instantiate(prefab, transform);
            }
            else
            {
                GameObject ballObject = new GameObject("PhantomBall");
                ballObject.transform.SetParent(transform);
                ballObject.AddComponent<SpriteRenderer>();
                ballObject.AddComponent<Rigidbody2D>();
                ballObject.AddComponent<CircleCollider2D>();
                phantom = ballObject.AddComponent<BallController>();
            }

            phantom.Initialize(ballSprite, bounceMaterial, ballRadius * 0.86f, maxBallSpeed);
            Color color = owner == Team.Blue ? new Color(0.35f, 0.75f, 1f, 0.7f) : new Color(1f, 0.35f, 0.35f, 0.7f);
            phantom.ConfigurePhantom(owner, phantomBallMaxBounces, color);
            phantom.LaunchPhantom(origin + direction.normalized * phantomBallSpawnOffset, direction, speed);
            Collider2D phantomCollider = phantom.GetComponent<Collider2D>();
            Collider2D mainBallCollider = ball != null ? ball.GetComponent<Collider2D>() : null;
            if (phantomCollider != null && mainBallCollider != null)
            {
                Physics2D.IgnoreCollision(phantomCollider, mainBallCollider);
            }

            phantomBalls.Add(phantom);
        }

        private void UpdatePhantomBallInterceptions()
        {
            for (int i = phantomBalls.Count - 1; i >= 0; i--)
            {
                BallController phantom = phantomBalls[i];
                if (phantom == null)
                {
                    phantomBalls.RemoveAt(i);
                    continue;
                }

                List<PlayerAgent> interceptors = phantom.PhantomOwner == Team.Blue ? redPlayers : bluePlayers;
                foreach (PlayerAgent player in interceptors)
                {
                    if (!CanPlayerInterceptPhantom(player, phantom))
                    {
                        continue;
                    }

                    Destroy(phantom.gameObject);
                    phantomBalls.RemoveAt(i);
                    ShowMessage($"{GetTeamLabel(player.Team)}拦截幻影足球");
                    break;
                }
            }
        }

        private void ClearPhantomBalls()
        {
            for (int i = phantomBalls.Count - 1; i >= 0; i--)
            {
                if (phantomBalls[i] != null)
                {
                    Destroy(phantomBalls[i].gameObject);
                }
            }

            phantomBalls.Clear();
        }

        private void DestroyActivePhantomPlayer()
        {
            if (activePhantomPlayer == null)
            {
                return;
            }

            Destroy(activePhantomPlayer.gameObject);
            activePhantomPlayer = null;
        }

        private void ClearExtraPlayers()
        {
            for (int i = extraPlayers.Count - 1; i >= 0; i--)
            {
                PlayerAgent player = extraPlayers[i];
                if (player == null)
                {
                    continue;
                }

                bluePlayers.Remove(player);
                redPlayers.Remove(player);
                Destroy(player.gameObject);
            }

            extraPlayers.Clear();
        }

        private void ResetTalents()
        {
            for (int i = 0; i < blueTalentCounts.Length; i++)
            {
                blueTalentCounts[i] = 0;
                redTalentCounts[i] = 0;
            }

            SyncAllControlRangeVisuals();
            UpdateTalentUi();
        }

        private bool HasTalent(Team team, TalentId talentId)
        {
            return GetTalentCount(team, talentId) > 0;
        }

        private bool CanGainTalent(Team team, TalentId talentId)
        {
            return GetTalentCount(team, talentId) < GetTalentMaxCount(talentId);
        }

        private int GetTalentMaxCount(TalentId talentId)
        {
            switch (talentId)
            {
                case TalentId.PhantomFootball:
                case TalentId.BigfootForward:
                case TalentId.ShieldField:
                case TalentId.PassField:
                    return 3;
                case TalentId.ExtraForward:
                case TalentId.ExtraMidfielder:
                case TalentId.ExtraDefender:
                    return 2;
                case TalentId.PhantomPlayer:
                    return 1;
                default:
                    return 1;
            }
        }

        private int GetTalentCount(Team team, TalentId talentId)
        {
            return GetTalentCounts(team)[(int)talentId];
        }

        private int[] GetTalentCounts(Team team)
        {
            return team == Team.Blue ? blueTalentCounts : redTalentCounts;
        }

        private void SyncAllControlRangeVisuals()
        {
            SyncTeamControlRangeVisuals(Team.Blue);
            SyncTeamControlRangeVisuals(Team.Red);
        }

        private void SyncTeamControlRangeVisuals(Team team)
        {
            List<PlayerAgent> players = team == Team.Blue ? bluePlayers : redPlayers;
            foreach (PlayerAgent player in players)
            {
                SyncPlayerControlRangeVisual(player);
            }
        }

        private void SyncPlayerControlRangeVisual(PlayerAgent player)
        {
            if (player == null)
            {
                return;
            }

            player.SetControlRangeMultiplier(GetEffectiveCatchRadius(player) / Mathf.Max(0.001f, player.CatchRadius));
        }

        private string GetTalentSummary(Team team)
        {
            int[] counts = GetTalentCounts(team);
            string summary = string.Empty;

            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] <= 0)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(summary))
                {
                    summary += "，";
                }

                summary += $"{GetTalentName((TalentId)i)}x{counts[i]}";
            }

            return string.IsNullOrEmpty(summary) ? "无" : summary;
        }

        private string GetTalentName(TalentId talentId)
        {
            switch (talentId)
            {
                case TalentId.PhantomFootball:
                    return "幻影足球";
                case TalentId.BigfootForward:
                    return "大脚怪前锋";
                case TalentId.ExtraForward:
                    return "额外前锋";
                case TalentId.ExtraMidfielder:
                    return "额外中场";
                case TalentId.ExtraDefender:
                    return "额外后卫";
                case TalentId.ShieldField:
                    return "护球立场";
                case TalentId.PassField:
                    return "传球立场";
                case TalentId.PhantomPlayer:
                    return "幻影球员";
                default:
                    return "香蕉球";
            }
        }

        private Color GetTalentColor(TalentId talentId)
        {
            switch (talentId)
            {
                case TalentId.PhantomFootball:
                    return new Color(0.42f, 0.72f, 1f, 1f);
                case TalentId.BigfootForward:
                    return new Color(1f, 0.6f, 0.18f, 1f);
                case TalentId.ExtraForward:
                    return new Color(1f, 0.28f, 0.22f, 1f);
                case TalentId.ExtraMidfielder:
                    return new Color(0.38f, 1f, 0.44f, 1f);
                case TalentId.ExtraDefender:
                    return new Color(0.35f, 0.58f, 1f, 1f);
                case TalentId.ShieldField:
                    return new Color(0.25f, 1f, 0.86f, 1f);
                case TalentId.PassField:
                    return new Color(0.78f, 0.45f, 1f, 1f);
                case TalentId.PhantomPlayer:
                    return new Color(0.42f, 0.92f, 1f, 1f);
                default:
                    return new Color(1f, 0.9f, 0.2f, 1f);
            }
        }

        private Vector2 Rotate(Vector2 direction, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos).normalized;
        }

        private void ShowMessage(string message)
        {
            messageText = message;
            messageUntil = Time.time + 1.5f;
        }

        private string GetTeamLabel(Team team)
        {
            return team == Team.Blue ? "蓝方" : "红方";
        }

        private bool IsHoldChargeArmed(Team team)
        {
            return team == Team.Blue ? blueHoldChargeArmed : redHoldChargeArmed;
        }

        private void SetHoldChargeArmed(Team team, bool armed)
        {
            if (team == Team.Blue)
            {
                blueHoldChargeArmed = armed;
            }
            else
            {
                redHoldChargeArmed = armed;
            }
        }

        private int GetPhantomPlayerUsesRemaining(Team team)
        {
            return team == Team.Blue ? bluePhantomPlayerUsesRemaining : redPhantomPlayerUsesRemaining;
        }

        private void SetPhantomPlayerUsesRemaining(Team team, int uses)
        {
            if (team == Team.Blue)
            {
                bluePhantomPlayerUsesRemaining = uses;
            }
            else
            {
                redPhantomPlayerUsesRemaining = uses;
            }
        }

        private bool IsPhantomPlayerPassWindowActive(Team team)
        {
            return team == Team.Blue ? bluePhantomPlayerPassWindowActive : redPhantomPlayerPassWindowActive;
        }

        private void SetPhantomPlayerPassWindowActive(Team team, bool active)
        {
            if (team == Team.Blue)
            {
                bluePhantomPlayerPassWindowActive = active;
            }
            else
            {
                redPhantomPlayerPassWindowActive = active;
            }
        }

        private Color GetPhantomPlayerColor(Team team)
        {
            return team == Team.Blue ? phantomPlayerColor : new Color(1f, 0.38f, 0.34f, phantomPlayerColor.a);
        }

        private void OnGUI()
        {
            DrawScoreboard();
            DrawGoalBanner();

            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 18,
                normal = { textColor = Color.white }
            };

            string defaultMessage = IsPvpMode()
                ? "蓝方：空格/Z/手柄1A-B；红方：Enter或右Ctrl/右Shift或/手柄2A-B；M 切 PVE"
                : "空格/手柄1A 抓球和蓄力，Z/手柄1B 召唤幻影球员，M 切 PVP，R 重置，N 重新开始";
            string message = Time.time <= messageUntil ? messageText : defaultMessage;
            float charge01 = Mathf.Clamp01(chargeTime / maxChargeTime);
            string text =
                $"《别让球停下来》Unity 基础原型\n" +
                $"模式：{GetMatchModeLabel()}\n" +
                $"蓝方 {blueScore} : {redScore} 红方\n" +
                $"状态：{stateText}\n" +
                $"蓄力：{charge01:P0}\n" +
                $"球速：{ball.Velocity.magnitude:0.0}\n" +
                $"{message}";

            GUI.Box(new Rect(16f, 86f, 460f, 188f), text, style);

            if (talentSelectionOpen)
            {
                DrawTalentSelection();
            }

            if (showInputDebug)
            {
                DrawInputDebug();
            }
        }

        private void DrawInputDebug()
        {
            GUIStyle debugStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 14,
                normal = { textColor = Color.white },
                wordWrap = false
            };

            string teamBinding = IsPvpMode()
                ? "PVP: 设备1=蓝方 / 设备2=红方"
                : "PVE: 任意设备=蓝方";
            string debugText = $"{teamBinding}\n{KeepBallNewInput.GetDebugText()}{KeepBallNewInput.GetLegacyDebugText()}";
            GUI.Box(new Rect(Screen.width - 560f, 86f, 544f, Mathf.Min(Screen.height - 110f, 420f)), debugText, debugStyle);
        }

        private void DrawGoalBanner()
        {
            if (string.IsNullOrEmpty(goalBannerText) || Time.unscaledTime > goalBannerUntilRealtime)
            {
                return;
            }

            float width = Mathf.Min(620f, Screen.width - 48f);
            float height = 96f;
            Rect bannerRect = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.22f, width, height);
            DrawFilledRect(bannerRect, new Color(0.02f, 0.025f, 0.03f, 0.72f));
            DrawBorder(bannerRect, goalBannerColor, 5f);

            GUIStyle bannerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 42,
                fontStyle = FontStyle.Bold,
                normal = { textColor = goalBannerColor }
            };

            GUI.Label(bannerRect, goalBannerText, bannerStyle);
        }

        private void DrawScoreboard()
        {
            GUIStyle scoreStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                normal = { textColor = Color.white }
            };

            float width = 520f;
            float height = 58f;
            Rect rect = new Rect((Screen.width - width) * 0.5f, 14f, width, height);
            string text = $"蓝方 {blueScore}  -  {redScore} 红方\n模式：{GetMatchModeLabel()} / 下一轮：{GetTeamLabel(nextKickoffTeam)}开球";
            GUI.Box(rect, text, scoreStyle);
        }

        private void DrawTalentSelection()
        {
            DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.05f, 0.05f, 0.055f, 0.62f));

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            string titleText = talentSecondPickPending
                ? $"{GetTeamLabel(currentTalentPickingTeam)}后选天赋"
                : $"{GetTeamLabel(currentTalentPickingTeam)}先进球，先选天赋";
            GUI.Label(new Rect(0f, 76f, Screen.width, 44f), titleText, titleStyle);

            GUIStyle infoStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                normal = { textColor = new Color(0.84f, 0.88f, 0.94f, 1f) }
            };

            string orderText = talentSecondPickPending
                ? $"{GetTeamLabel(currentTalentPickingTeam)}只能从剩余天赋中选择。上次：蓝方 {lastBlueTalentText} / 红方 {lastRedTalentText}"
                : $"进球方先选，失球方后选剩余天赋。上次：蓝方 {lastBlueTalentText} / 红方 {lastRedTalentText}";
            GUI.Label(new Rect(0f, 122f, Screen.width, 24f), orderText, infoStyle);
            string talentInputHint = currentTalentPickingTeam == Team.Blue
                ? "蓝方：手柄1左右选择，手柄1A确认；键盘方向键/鼠标也可选择"
                : "红方：手柄2左右选择，手柄2A确认；键盘方向键/鼠标也可选择";
            GUI.Label(new Rect(0f, 146f, Screen.width, 22f), talentInputHint, infoStyle);

            float cardWidth = 240f;
            float cardHeight = 380f;
            float gap = 28f;
            float totalWidth = cardWidth * TalentChoiceCount + gap * (TalentChoiceCount - 1);
            float startX = (Screen.width - totalWidth) * 0.5f;
            float cardY = Mathf.Max(170f, (Screen.height - cardHeight) * 0.5f + 28f);

            GUIStyle cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            GUIStyle descriptionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 15,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            for (int i = 0; i < TalentChoiceCount; i++)
            {
                TalentOption option = currentTalentOptions[i];
                Rect cardRect = new Rect(startX + i * (cardWidth + gap), cardY, cardWidth, cardHeight);
                bool selectable = CanSelectTalentOption(i, currentTalentPickingTeam);
                if (selectable && cardRect.Contains(Event.current.mousePosition))
                {
                    selectedTalentIndex = i;
                }

                DrawTalentCard(cardRect, option, cardTitleStyle, descriptionStyle, i == selectedTalentIndex, talentOptionTaken[i], selectable, currentTalentPickingTeam);

                if (Event.current.type == EventType.MouseUp && cardRect.Contains(Event.current.mousePosition))
                {
                    SelectTalent(i);
                    Event.current.Use();
                }
            }
        }

        private void DrawTalentCard(Rect cardRect, TalentOption option, GUIStyle titleStyle, GUIStyle descriptionStyle, bool selected, bool taken, bool selectable, Team viewingTeam)
        {
            Color cardColor = selected && selectable ? new Color(0.13f, 0.16f, 0.22f, 1f) : new Color(0.09f, 0.11f, 0.15f, 1f);
            Color borderColor = selected && selectable ? option.AccentColor : new Color(0.22f, 0.27f, 0.36f, 1f);
            DrawFilledRect(cardRect, cardColor);
            DrawBorder(cardRect, borderColor, selected && selectable ? 7f : 4f);
            DrawBorder(new Rect(cardRect.x + 6f, cardRect.y + 6f, cardRect.width - 12f, cardRect.height - 12f), new Color(0.04f, 0.05f, 0.07f, 1f), 2f);
            if (selected && selectable)
            {
                DrawBorder(new Rect(cardRect.x + 10f, cardRect.y + 10f, cardRect.width - 20f, cardRect.height - 20f), new Color(1f, 1f, 1f, 0.45f), 2f);
            }

            GUI.Label(new Rect(cardRect.x + 12f, cardRect.y + 12f, cardRect.width - 24f, 36f), option.Name, titleStyle);

            Rect iconRect = new Rect(cardRect.x + 16f, cardRect.y + 54f, cardRect.width - 32f, 142f);
            DrawFilledRect(iconRect, Color.black);
            DrawBorder(iconRect, new Color(0.02f, 0.025f, 0.035f, 1f), 3f);
            DrawTalentIcon(iconRect, option);
            DrawIconPlaceholderLabel(iconRect);

            Rect descriptionRect = new Rect(cardRect.x + 16f, cardRect.y + 208f, cardRect.width - 32f, 122f);
            string description = $"{option.Description}\n\n{GetTeamLabel(viewingTeam)}当前拥有：{GetTalentCount(viewingTeam, option.Id)} / {GetTalentMaxCount(option.Id)}";
            GUI.Label(descriptionRect, description, descriptionStyle);

            GUIStyle chooseStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                normal = { textColor = new Color(0.78f, 0.84f, 0.95f, 1f) }
            };

            string confirmText = viewingTeam == Team.Blue ? "手柄1A确认 / 点击选择" : "手柄2A确认 / 点击选择";
            string actionText = taken ? "已被选择" : selectable ? selected ? confirmText : "摇杆左右切换" : "已满";
            GUI.Label(new Rect(cardRect.x + 16f, cardRect.yMax - 36f, cardRect.width - 32f, 22f), actionText, chooseStyle);

            if (!selectable)
            {
                DrawFilledRect(cardRect, new Color(0f, 0f, 0f, 0.42f));
            }
        }

        private void DrawIconPlaceholderLabel(Rect iconRect)
        {
            GUIStyle iconStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerRight,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 1f, 1f, 0.7f) }
            };

            GUI.Label(new Rect(iconRect.x + 8f, iconRect.yMax - 28f, iconRect.width - 16f, 20f), "ICON", iconStyle);
        }

        private void DrawTalentIcon(Rect iconRect, TalentOption option)
        {
            switch (option.Id)
            {
                case TalentId.PhantomFootball:
                    DrawFilledRect(new Rect(iconRect.center.x - 18f, iconRect.center.y - 18f, 36f, 36f), option.AccentColor);
                    DrawFilledRect(new Rect(iconRect.center.x - 58f, iconRect.center.y - 12f, 24f, 24f), new Color(option.AccentColor.r, option.AccentColor.g, option.AccentColor.b, 0.45f));
                    DrawFilledRect(new Rect(iconRect.center.x + 38f, iconRect.center.y - 12f, 24f, 24f), new Color(option.AccentColor.r, option.AccentColor.g, option.AccentColor.b, 0.45f));
                    DrawFilledRect(new Rect(iconRect.x + 42f, iconRect.center.y + 30f, iconRect.width - 84f, 4f), new Color(1f, 1f, 1f, 0.35f));
                    break;
                case TalentId.BigfootForward:
                    DrawFilledRect(new Rect(iconRect.center.x - 36f, iconRect.center.y - 12f, 74f, 42f), option.AccentColor);
                    DrawFilledRect(new Rect(iconRect.center.x + 22f, iconRect.center.y - 28f, 34f, 22f), option.AccentColor);
                    DrawFilledRect(new Rect(iconRect.x + 40f, iconRect.y + 28f, iconRect.width - 80f, 6f), new Color(option.AccentColor.r, option.AccentColor.g, option.AccentColor.b, 0.45f));
                    DrawFilledRect(new Rect(iconRect.x + 40f, iconRect.yMax - 34f, iconRect.width - 80f, 6f), new Color(option.AccentColor.r, option.AccentColor.g, option.AccentColor.b, 0.45f));
                    break;
                case TalentId.ExtraForward:
                case TalentId.ExtraMidfielder:
                case TalentId.ExtraDefender:
                    float lineY = option.Id == TalentId.ExtraForward ? iconRect.y + 36f : option.Id == TalentId.ExtraMidfielder ? iconRect.center.y : iconRect.yMax - 40f;
                    DrawFilledRect(new Rect(iconRect.x + 36f, lineY, iconRect.width - 72f, 5f), new Color(option.AccentColor.r, option.AccentColor.g, option.AccentColor.b, 0.5f));
                    DrawFilledRect(new Rect(iconRect.center.x - 20f, lineY - 18f, 40f, 40f), option.AccentColor);
                    DrawFilledRect(new Rect(iconRect.center.x + 34f, lineY - 10f, 20f, 20f), new Color(1f, 1f, 1f, 0.85f));
                    break;
                case TalentId.ShieldField:
                case TalentId.PassField:
                    DrawBorder(new Rect(iconRect.center.x - 52f, iconRect.center.y - 52f, 104f, 104f), new Color(option.AccentColor.r, option.AccentColor.g, option.AccentColor.b, 0.45f), 4f);
                    DrawBorder(new Rect(iconRect.center.x - 34f, iconRect.center.y - 34f, 68f, 68f), new Color(option.AccentColor.r, option.AccentColor.g, option.AccentColor.b, 0.7f), 4f);
                    DrawFilledRect(new Rect(iconRect.center.x - 18f, iconRect.center.y - 18f, 36f, 36f), option.AccentColor);
                    break;
                case TalentId.PhantomPlayer:
                    DrawBorder(new Rect(iconRect.center.x - 54f, iconRect.center.y - 38f, 108f, 76f), new Color(option.AccentColor.r, option.AccentColor.g, option.AccentColor.b, 0.55f), 4f);
                    DrawFilledRect(new Rect(iconRect.center.x - 18f, iconRect.center.y - 18f, 36f, 36f), option.AccentColor);
                    DrawFilledRect(new Rect(iconRect.center.x + 34f, iconRect.center.y - 10f, 20f, 20f), new Color(1f, 1f, 1f, 0.85f));
                    DrawFilledRect(new Rect(iconRect.center.x - 54f, iconRect.center.y - 2f, 24f, 4f), new Color(option.AccentColor.r, option.AccentColor.g, option.AccentColor.b, 0.75f));
                    DrawFilledRect(new Rect(iconRect.center.x + 14f, iconRect.center.y - 2f, 24f, 4f), new Color(option.AccentColor.r, option.AccentColor.g, option.AccentColor.b, 0.75f));
                    break;
                default:
                    for (int i = 0; i < 6; i++)
                    {
                        float x = iconRect.x + 44f + i * 24f;
                        float y = iconRect.center.y + Mathf.Sin(i * 0.8f) * 28f;
                        DrawFilledRect(new Rect(x, y, 22f, 8f), option.AccentColor);
                    }

                    DrawFilledRect(new Rect(iconRect.x + 52f, iconRect.y + 30f, 4f, 26f), new Color(option.AccentColor.r, option.AccentColor.g, option.AccentColor.b, 0.65f));
                    DrawFilledRect(new Rect(iconRect.xMax - 56f, iconRect.yMax - 56f, 4f, 26f), new Color(option.AccentColor.r, option.AccentColor.g, option.AccentColor.b, 0.65f));
                    break;
            }
        }

        private void DrawFilledRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private void DrawBorder(Rect rect, Color color, float thickness)
        {
            DrawFilledRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawFilledRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawFilledRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawFilledRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }
    }
}
