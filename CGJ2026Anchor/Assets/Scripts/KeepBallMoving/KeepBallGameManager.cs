using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeepBallMoving
{
    public sealed class KeepBallGameManager : MonoBehaviour
    {
        [Header("Field")]
        [SerializeField] private float fieldWidth = 30f;
        [SerializeField] private float fieldHeight = 17f;
        [SerializeField] private float wallThickness = 0.5f;
        [SerializeField] private float goalDepth = 1f;
        [SerializeField] private float goalHeight = 4.5f;

        [Header("Actors")]
        [SerializeField] private int playersPerTeam = 5;
        [SerializeField] private float playerRadius = 0.5f;
        [SerializeField] private float ballRadius = 0.28f;
        [SerializeField] private float catchRadius = 1.8f;
        [SerializeField] private float holdPointRadius = 0.18f;

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
        [SerializeField] private float redAutoHoldDelay = 0.55f;
        [SerializeField] private float redReleaseSpeed = 13.5f;

        private readonly List<PlayerAgent> bluePlayers = new List<PlayerAgent>();
        private readonly List<PlayerAgent> redPlayers = new List<PlayerAgent>();

        private BallController ball;
        private Sprite squareSprite;
        private Sprite bluePlayerSprite;
        private Sprite redPlayerSprite;
        private Sprite controlRangeSprite;
        private Sprite holdPointSprite;
        private Sprite ballSprite;
        private Sprite centerRingSprite;
        private PhysicsMaterial2D bounceMaterial;
        private float chargeTime;
        private bool goalLocked;
        private int blueScore;
        private int redScore;
        private string stateText = "空格抓球";
        private string messageText = "基础原型：空格抓球，松开释放";
        private float messageUntil;
        private float autoControlCooldownUntil;

        private Color fieldColor = new Color(0.08f, 0.42f, 0.19f, 1f);
        private Color lineColor = new Color(0.93f, 0.97f, 0.93f, 1f);
        private Color blueColor = new Color(0.05f, 0.33f, 1f, 1f);
        private Color redColor = new Color(0.95f, 0.12f, 0.11f, 1f);

        private void Awake()
        {
            Physics2D.gravity = Vector2.zero;
            CreateRuntimeAssets();
            SetupCamera();
            BuildField();
            SpawnPlayers();
            SpawnBall();
            ResetRound();
        }

        private void Update()
        {
            if (goalLocked || ball == null)
            {
                return;
            }

            UpdatePlayerMovement(Time.deltaTime);

            if (ball.State == BallState.Held)
            {
                chargeTime += Time.deltaTime;
                ball.TickHold(Time.deltaTime);

                if (ball.Holder != null && ball.Holder.Team == Team.Red)
                {
                    stateText = $"红方控球 {Mathf.Clamp01(chargeTime / redAutoHoldDelay):P0}";

                    if (chargeTime >= redAutoHoldDelay)
                    {
                        ReleaseRedHeldBall();
                    }

                    return;
                }

                stateText = $"蓝方蓄力 {Mathf.Clamp01(chargeTime / maxChargeTime):P0}";

                if (Input.GetKeyUp(KeyCode.Space))
                {
                    ReleaseHeldBall();
                }

                return;
            }

            UpdateControlHighlights();
            stateText = FindNearestCatchablePlayer() != null ? "可抓球：按住空格" : "等待球靠近蓝方";

            if (TryAutoRedCatchBall())
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryCatchBall();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetRound();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                RestartMatch();
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

            bounceMaterial = new PhysicsMaterial2D("KeepBall_Bouncy")
            {
                friction = 0f,
                bounciness = 0.88f
            };
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
        }

        private void BuildField()
        {
            GameObject fieldRoot = new GameObject("Field");
            fieldRoot.transform.SetParent(transform);

            CreateVisualRect("Grass", Vector2.zero, new Vector2(fieldWidth, fieldHeight), fieldColor, -10, fieldRoot.transform);
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

        private GameObject CreateVisualRect(string name, Vector2 position, Vector2 size, Color color, int sortingOrder, Transform parent)
        {
            GameObject rect = new GameObject(name);
            rect.transform.SetParent(parent);
            rect.transform.position = position;
            rect.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = rect.AddComponent<SpriteRenderer>();
            renderer.sprite = squareSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
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
            GameObject playersRoot = new GameObject("Players");
            playersRoot.transform.SetParent(transform);

            for (int i = 0; i < playersPerTeam; i++)
            {
                PlayerRole role = GetRoleForIndex(i, playersPerTeam);
                Vector2 bluePosition = GetPlayerSpawnPosition(Team.Blue, role, i, playersPerTeam);
                PlayerAgent blue = CreatePlayer(Team.Blue, role, i + 1, bluePosition, playersRoot.transform);
                bluePlayers.Add(blue);

                Vector2 redPosition = GetPlayerSpawnPosition(Team.Red, role, i, playersPerTeam);
                PlayerAgent red = CreatePlayer(Team.Red, role, i + 1, redPosition, playersRoot.transform);
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

        private PlayerAgent CreatePlayer(Team team, PlayerRole role, int index, Vector2 position, Transform parent)
        {
            GameObject playerObject = new GameObject();
            playerObject.transform.SetParent(parent);

            playerObject.AddComponent<SpriteRenderer>();
            playerObject.AddComponent<CircleCollider2D>();
            PlayerAgent player = playerObject.AddComponent<PlayerAgent>();
            player.Initialize(
                team,
                role,
                index,
                position,
                team == Team.Blue ? bluePlayerSprite : redPlayerSprite,
                controlRangeSprite,
                holdPointSprite,
                team == Team.Blue ? blueColor : redColor,
                playerRadius,
                catchRadius,
                holdPointRadius);

            return player;
        }

        private void SpawnBall()
        {
            GameObject ballObject = new GameObject("Ball");
            ballObject.transform.SetParent(transform);
            ballObject.AddComponent<SpriteRenderer>();
            ballObject.AddComponent<Rigidbody2D>();
            ballObject.AddComponent<CircleCollider2D>();

            ball = ballObject.AddComponent<BallController>();
            ball.Initialize(ballSprite, bounceMaterial, ballRadius, maxBallSpeed);
        }

        private void RestartMatch()
        {
            blueScore = 0;
            redScore = 0;
            ResetRound();
            ShowMessage("重新开始");
        }

        private void ResetRound()
        {
            goalLocked = false;
            chargeTime = 0f;
            autoControlCooldownUntil = 0f;

            foreach (PlayerAgent player in bluePlayers)
            {
                player.ResetToSpawn();
            }

            foreach (PlayerAgent player in redPlayers)
            {
                player.ResetToSpawn();
            }

            Vector2 startPosition = new Vector2(-fieldWidth * 0.25f, 0f);
            Vector2 startVelocity = new Vector2(8.5f, Random.Range(-2.2f, 2.2f));
            ball.ResetBall(startPosition, startVelocity);
            UpdateControlHighlights();
        }

        private void TryCatchBall()
        {
            PlayerAgent catcher = FindNearestCatchablePlayer();
            if (catcher == null)
            {
                ShowMessage("没有蓝方在抓球范围内");
                return;
            }

            chargeTime = 0f;
            ball.BeginHold(catcher, minHoldRadius, holdAngularSpeed);
            ShowMessage($"抓住！{catcher.name}");
        }

        private bool TryAutoRedCatchBall()
        {
            PlayerAgent redCatcher = FindNearestControllingPlayer(redPlayers);
            if (redCatcher == null)
            {
                return false;
            }

            chargeTime = 0f;
            ball.BeginHold(redCatcher, minHoldRadius, -holdAngularSpeed);
            ShowMessage($"红方拿球：{redCatcher.name}");
            return true;
        }

        private void ReleaseHeldBall()
        {
            float charge01 = Mathf.Clamp01(chargeTime / maxChargeTime);
            float releaseSpeed = Mathf.Lerp(minReleaseSpeed, maxReleaseSpeed, charge01);
            ball.Release(releaseSpeed);
            ShowMessage(charge01 >= 0.95f ? "爆射！" : "释放！");
            chargeTime = 0f;
            autoControlCooldownUntil = Time.time + 0.35f;
        }

        private void ReleaseRedHeldBall()
        {
            Vector2 leftGoalCenter = new Vector2(-fieldWidth * 0.5f - goalDepth, Random.Range(-goalHeight * 0.25f, goalHeight * 0.25f));
            ball.ReleaseToward(leftGoalCenter, redReleaseSpeed);
            ShowMessage("红方传出！");
            chargeTime = 0f;
            autoControlCooldownUntil = Time.time + 0.45f;
        }

        private PlayerAgent FindNearestCatchablePlayer()
        {
            PlayerAgent nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (PlayerAgent player in bluePlayers)
            {
                float distance = Vector2.Distance(player.Position, ball.Position);
                if (player.CanCatch(ball) && distance < nearestDistance)
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
                if (player.CanControlBall(ball) && distance < nearestDistance)
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
                player.SetCatchHighlighted(player.CanCatch(ball));
            }

            foreach (PlayerAgent player in redPlayers)
            {
                player.SetCatchHighlighted(ball.State == BallState.Free && player.CanControlBall(ball));
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
            ball.BeginHold(stealer, minHoldRadius, stealer.Team == Team.Red ? -holdAngularSpeed : holdAngularSpeed);
            autoControlCooldownUntil = Time.time + 0.2f;
            ShowMessage($"{(stealer.Team == Team.Blue ? "蓝方" : "红方")}抢断！");
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
                MovePlayer(player, target, redPlayers, redMoveSpeed, deltaTime);
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
                    return GetForwardTarget(index, attackDirection);
                case PlayerRole.Defender:
                    return GetDefenderTarget(index, side);
                default:
                    return GetMidfielderTarget(player, index, team, side, attackDirection);
            }
        }

        private Vector2 GetForwardTarget(int index, float attackDirection)
        {
            float lane = GetRoleLaneForIndex(index, playersPerTeam, PlayerRole.Forward, fieldHeight * 0.31f);
            float advancedX = attackDirection * fieldWidth * 0.28f;
            float ballAheadX = ball.Position.x + attackDirection * 4.2f;
            float x = attackDirection > 0f ? Mathf.Max(advancedX, ballAheadX) : Mathf.Min(advancedX, ballAheadX);
            x = Mathf.Clamp(x, -fieldWidth * 0.43f, fieldWidth * 0.43f);

            float y = Mathf.Lerp(lane, ball.Position.y * 0.45f + lane * 0.55f, 0.45f);
            return ClampInsideField(new Vector2(x, y));
        }

        private Vector2 GetDefenderTarget(int index, float side)
        {
            float lane = GetRoleLaneForIndex(index, playersPerTeam, PlayerRole.Defender, fieldHeight * 0.28f);
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
            float lane = GetRoleLaneForIndex(index, playersPerTeam, PlayerRole.Midfielder, fieldHeight * 0.27f);
            float sideOffset = ballRank % 2 == 0 ? -1.35f : 1.35f;
            float xOffset = attackDirection * (ballRank == 0 ? -0.7f : 0.9f);
            Vector2 nearBallTarget = ballPosition + new Vector2(xOffset, sideOffset);

            float midfieldX = side * fieldWidth * 0.06f;
            Vector2 shapeTarget = new Vector2(midfieldX, lane);
            return ClampInsideField(Vector2.Lerp(shapeTarget, nearBallTarget, 0.72f));
        }

        private void MovePlayer(PlayerAgent player, Vector2 target, List<PlayerAgent> teamPlayers, float speed, float deltaTime)
        {
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

        public void OnGoal(GoalSide side)
        {
            if (goalLocked)
            {
                return;
            }

            goalLocked = true;

            if (side == GoalSide.Right)
            {
                blueScore++;
                ShowMessage("蓝方进球！");
            }
            else
            {
                redScore++;
                ShowMessage("红方进球！");
            }

            StartCoroutine(ResetAfterGoal());
        }

        private IEnumerator ResetAfterGoal()
        {
            yield return new WaitForSeconds(1f);
            ResetRound();
        }

        private void ShowMessage(string message)
        {
            messageText = message;
            messageUntil = Time.time + 1.5f;
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 18,
                normal = { textColor = Color.white }
            };

            string message = Time.time <= messageUntil ? messageText : "空格抓球 / 松开释放，R 重置，N 重新开始";
            float charge01 = Mathf.Clamp01(chargeTime / maxChargeTime);
            string text =
                $"《别让球停下来》Unity 基础原型\n" +
                $"蓝方 {blueScore} : {redScore} 红方\n" +
                $"状态：{stateText}\n" +
                $"蓄力：{charge01:P0}\n" +
                $"球速：{ball.Velocity.magnitude:0.0}\n" +
                $"{message}";

            GUI.Box(new Rect(16f, 16f, 360f, 170f), text, style);
        }
    }
}
