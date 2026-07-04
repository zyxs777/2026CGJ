# 《别让球停下来》Unity 原型 ReadMe

## 1. 项目目标

《别让球停下来》是一个 Unity 2D 顶视角足球原型。当前目标不是完整足球模拟，而是验证一个快速、可玩的核心循环：球不能轻易停下，玩家通过抓球、蓄力、传球、射门和天赋选择来对抗红方 AI，或在 PVP 模式下让红方也由玩家控制。

核心循环：

```text
球在大球场里持续运动
蓝方和红方球员各自跑位
蓝方由玩家控制抓球和释放
PVE 下红方由 AI 自动接球、传球和射门
PVP 下红方由另一名玩家控制抓球和释放
进球后计分并进入三选一天赋
选完天赋后由失球方开球
```

当前优先级：

```text
先保证物理、控球、传球、射门、进球、回合重置和模式切换可玩
再逐步完善 AI 强度、天赋组合、视觉表现和手感
```

## 2. 当前技术方案

当前使用 Unity 2D 实现。

核心技术：

- `Rigidbody2D`：足球物理运动。
- `CircleCollider2D`：足球、球员和范围检测。
- `BoxCollider2D`：球场边界和球门触发区域。
- 运行时 Sprite：生成球场、球员、足球、范围圈、天赋 icon 等基础图形。
- Resources Prefab：球员、足球、幻影足球、震荡波、幻影球员和天赋 UI 都可以通过预制体替换。
- `OnGUI`：临时显示比分、状态、三选一天赋界面和提示信息。
- Unity UI：左右下角天赋列表使用运行时 Canvas 和 `KeepBallTalentBadge` 预制体显示。

当前原型不依赖手动摆场景。`KeepBallBootstrapper` 会在场景加载后自动创建 `KeepBallGameManager`。打开 Unity 场景后直接 Play，原型会自动生成：

```text
球场
足球
蓝方球员
红方球员
左右球门
比分 UI
天赋选择 UI
左右下角双方天赋列表 UI
```

## 3. 主要脚本结构

主要代码目录：

```text
Assets/Scripts/KeepBallMoving/
```

### 3.1 KeepBallTypes.cs

定义基础枚举：

```csharp
Team
GoalSide
BallState
PlayerRole
```

阵营：

```text
Blue
Red
```

球员职责：

```text
Forward     前锋
Midfielder  中场
Defender    后卫
```

### 3.2 KeepBallBootstrapper.cs

运行时自动创建 `KeepBallGameManager`。

作用：

```text
如果场景里没有 KeepBallGameManager
则自动创建一个
```

### 3.3 KeepBallGameManager.cs

当前最核心的管理类，负责：

- 创建运行时素材。
- 设置相机。
- 生成球场、边界、球门。
- 生成蓝方和红方球员。
- 生成足球。
- 处理玩家输入。
- 处理蓝方抓球、蓄力和释放。
- 处理红方自动接球、传球和射门。
- 处理球员基础跑位。
- 处理进球计分和下一轮开球方。
- 处理固定开球位置和开球球员锁定。
- 处理三选一天赋。
- 处理天赋效果、天赋层数和上限。
- 刷新左右下角双方天赋 UI。
- 显示临时 UI。

### 3.4 PlayerAgent.cs

负责单个球员的数据和表现。

当前包含：

- 阵营 `Team`。
- 职责 `PlayerRole`。
- 接球范围 `CatchRadius`。
- 身体半径 `BodyRadius`。
- 出生位置。
- 控球范围圈显示。
- 临时高亮。
- 平滑击退。
- 基础移动接口。

### 3.5 BallController.cs

负责足球状态和物理。

当前包含：

- 自由球状态。
- Held 状态。
- 重置足球。
- 限制最大和最小球速。
- Held 时围绕持球球员旋转。
- 蓝方释放。
- 红方朝目标释放。
- 幻影足球配置。
- 幻影足球弹射次数统计。
- 香蕉球曲线支持预留。

Hold 半径规则：

```text
球进入控制范围后开始 Hold
记录当时球到球员中心的距离
这个距离作为绕圈半径
如果距离太小，则使用 minHoldRadius
```

### 3.6 GoalTrigger.cs

负责球门检测。

规则：

```text
球进入右侧球门 -> 蓝方得分
球进入左侧球门 -> 红方得分
```

进球后进入天赋选择，选完后由失球方开球。

### 3.7 RuntimeSpriteFactory.cs

负责运行时生成简单素材。

当前生成：

- 方形 Sprite。
- 圆形 Sprite。
- 圆环 Sprite。
- 天赋 icon Sprite。

用于：

- 球场底色。
- 球场线条。
- 球员。
- 足球。
- 控球范围圈。
- 天赋 UI 图标。
- fallback 特效。

### 3.8 ShockwaveEffect.cs

负责立场震荡波的视觉表现。

实际击退由 `KeepBallGameManager` 计算；`ShockwaveEffect` 只负责预制体视觉的扩散、淡出和销毁。

### 3.9 KeepBallTalentBadge.cs

负责左右下角单个天赋条目的 UI。

当前显示：

```text
天赋 icon
天赋名字
当前层数 / 最大层数
```

蓝方显示在左下角，红方显示在右下角，按当前已获得天赋依次向上排列。

## 4. 当前已实现内容

### 4.1 基础球场

已经实现顶视角大球场。

包含：

- 绿色球场背景。
- 上下左右边界。
- 中线。
- 中圈。
- 左右禁区线。
- 左右球门。
- 球场边界反弹。

默认尺寸：

```text
fieldWidth  = 30
fieldHeight = 17
```

球场背景支持替换为 Sprite 贴图：

```text
Assets/Resources/KeepBallMoving/KeepBallField.prefab
```

`KeepBallGameManager` 是运行时由 `KeepBallBootstrapper` 创建的，不需要提前挂在场景或预制体上。球场视觉改为预制体方式：

- 运行时会优先加载 `Assets/Resources/KeepBallMoving/KeepBallField.prefab`。
- prefab 里默认有一个 `Grass` 子物体，挂 `SpriteRenderer`。
- 提前打开这个 prefab，就可以直接配置 `Grass` 的 Sprite、Color、Transform、Sorting Order 等。
- 默认 `Grass` 使用 `Assets/Resources/KeepBallMoving/BG2.png`。

如果 `KeepBallField.prefab` 不存在，代码才会回退到旧的运行时 Grass 生成逻辑。中线、中圈、禁区线、墙体碰撞和球门逻辑仍由代码生成并叠在球场预制体上方。

### 4.2 足球物理

足球使用 `Rigidbody2D`。

当前行为：

- 无重力。
- 在球场内反弹。
- 有最大速度限制。
- 有最小速度保护，避免球太快停死。
- 进入球门后触发得分。
- 被 Hold 时切换为 Kinematic。
- 释放后切回 Dynamic 并设置速度。

### 4.3 双方球员

默认自动生成蓝方和红方球员。

默认每队人数：

```text
playersPerTeam = 5
```

球员职责包括：

```text
前锋
中场
后卫
```

球员有控制范围圈。大脚怪前锋天赋会同步更新前锋的范围圈显示。

球员预制体可以挂 Animator。`PlayerAgent` 会自动写入两个 bool 参数：

```text
run：不持球且位置发生移动时为 true；不持球且停止时为 false
hold：持球时为 true；不持球时为 false
```

如果预制体没有 Animator，或 Animator 没有对应参数，代码会静默跳过。

球员 Sprite 默认朝右。运行时向左移动会设置 `flipX=true`，向右移动会恢复 `flipX=false`，原地或竖向移动时保持当前朝向。

球员颜色也在球员预制体的 `PlayerAgent` 上调：

```text
blueColor / redColor：普通颜色
blueHighlightColor / redHighlightColor：真正持球时的高亮颜色
```

只有真正持球的球员会使用高亮颜色和放大效果；仅仅进入可接球范围不会高亮。持球球员 Sprite 的 `sortingOrder` 会设为 100，其他球员为 30。

蓝方默认使用 `KeepBallPlayer.prefab`，红方默认使用 `KeepBallPlayer1.prefab`，所以两队可以分别保存不同颜色和动画。

### 4.4 PVE / PVP 模式

当前支持两种模式：

```text
PVE：蓝方玩家控制，红方 AI 控制。
PVP：蓝方玩家控制，红方玩家控制。
```

默认模式是 `PVE`。可以在 `KeepBallGameManager` 的 Inspector 中修改 `matchMode`，也可以在游戏中按 `M` 切换模式。切换模式会重新开始比赛并清空当前比分和天赋。

隔离规则：

- PVE 下红方继续使用原有 AI 自动接球、蓄力、传球和射门。
- PVP 下红方 AI 自动接球和自动出球关闭，红方玩家接管抓球、蓄力、释放和幻影球员。
- PVP 下红方球员仍保留自动跑位，但移动速度不再受红方 AI 强度参数影响。
- PVE 和 PVP 共用球员、球、天赋、进球和回合重置逻辑。

### 4.5 玩家操作

键盘操作：

```text
蓝方空格：抓球 / 开始蓄力 / 确认踢出幻影球员持有的球
蓝方松开空格：释放足球
蓝方 Z：拥有幻影球员天赋后，在当前传球窗口中生成幻影球员
红方 Enter 或右 Ctrl：PVP 下抓球 / 开始蓄力 / 确认踢出幻影球员持有的球
红方松开 Enter 或右 Ctrl：PVP 下释放足球
红方右 Shift 或 /：PVP 下生成幻影球员
M：切换 PVE / PVP 并重新开始
F2：显示 / 隐藏手柄输入调试面板
R：重置当前回合
N：重新开始比赛并清空天赋
方向键左右：三选一天赋界面切换选择
鼠标点击：选择天赋
```

手柄操作：

```text
手柄1 A：蓝方抓球 / 蓄力 / 释放 / 确认蓝方天赋
手柄1 B：蓝方召唤幻影球员
手柄2 A：PVP 下红方抓球 / 蓄力 / 释放 / 确认红方天赋
手柄2 B：PVP 下红方召唤幻影球员
手柄1 左右摇杆：蓝方选择天赋
手柄2 左右摇杆：红方选择天赋
PVE 下仍兼容任意手柄 A/B 操作蓝方
```

当前已接入 Unity New Input System：

```text
com.unity.inputsystem = 1.7.0
Active Input Handling = Both
```

New Input System 可用时，代码会优先使用 `Gamepad.all`，并兼容部分只出现在 `Joystick.all` 的泛用手柄：

```text
Gamepad.all[0] -> 蓝方
Gamepad.all[1] -> 红方
buttonSouth -> A / 确认 / 抓球
buttonEast -> B / 召唤幻影球员
leftStick.x / rightStick.x -> 天赋左右选择

如果设备只被识别为 Joystick：
trigger / button0 -> A / 确认 / 抓球
button1 / button2 -> B / 召唤幻影球员
stick.x -> 天赋左右选择
```

PVE 下蓝方会读取任意已连接手柄，方便单手柄试玩。PVP 下会固定读取第 1 个可分离设备给蓝方、第 2 个可分离设备给红方。

手柄读取优先级：

```text
1. Unity New Input System：Gamepad.all + Joystick.all
2. Rewired：ReInput.controllers.Joysticks
3. Unity 旧 Input Manager：只作为单手柄 / 蓝方 Any 兜底
```

如果手柄按键没有反应，运行时按 `F2` 打开输入调试面板，查看 New Input System 当前识别到的设备、A/B 对应的 control 名称和按下状态。
如果 New Input System 显示设备数为 0，但 Rewired 能看到两个 Joystick，PVP 会使用 Rewired 的第 1 / 第 2 个 Joystick 区分蓝红双方。
如果 New Input System 和 Rewired 都没有可分离设备，但 Legacy Input 里能看到手柄名称，PVP 会从 `Input.GetJoystickNames()` 中扫描 J1-J8，按第 1 个有名字的槽位给蓝方、第 2 个有名字的槽位给红方。PVE 下仍读取旧输入 Any，方便单手柄试玩。

旧 Input Manager 仍作为兜底。若 New Input System 没有启用，左右选择会继续尝试：

```text
Joystick1Horizontal / P1Horizontal
Joystick2Horizontal / P2Horizontal
```

如果这些轴没有配置，会退回现有的 `Horizontal` 和 `RightStickHorizontal`，不会报错。

### 4.6 红方 AI

PVE 模式下，红方会自动：

- 根据控制范围接球。
- 根据职责选择传球或射门。
- 带有可调 AI 强度参数。
- 传球和射门不再绝对精准，会受到误差、反应、决策和力度随机影响。

相关参数在 `KeepBallGameManager` 的 `Red AI Difficulty` 区域。

PVP 模式下，红方 AI 自动接球和自动出球不会执行，但红方球员仍然自动跑位。

### 4.7 比分和足球规则

上方 UI 显示蓝红比分。

进球规则：

```text
蓝方进球 -> 红方下一轮开球
红方进球 -> 蓝方下一轮开球
```

进球演出：

- 进球瞬间会锁定当前回合。
- `Time.timeScale` 降低，球和球员整体进入慢动作。
- 破门的球会冻结在进球位置，确保镜头演出期间可见。
- 破门位置会生成 `GoalEffect.prefab` 进球特效。
- 镜头快速移动并放大到破门的球所在位置。
- 屏幕中央显示“蓝方进球！”或“红方进球！”。
- 演出结束后镜头和时间缩放恢复，再进入三选一天赋选择。

每次重新发球时：

- 所有球员回到阵型位置。
- 开球球员固定在本方后卫中路开球点。
- 开球持球期间，开球球员不会被人数变化、阵型重新分布或移动 AI 推走。
- 开完球后，开球球员恢复正常移动。

### 4.8 天赋系统

每次进球后出现三选一天赋界面。

规则：

```text
进球方先从三个天赋中选择一个
失球方再从剩余天赋中选择一个
双方都选择完成后开始下一轮
```

天赋有最大层数限制。三选一会优先从先进球一方尚未满层的天赋池中抽取。若后期可选池不足 3 个，当前实现会从剩余可选池里重复补足 3 张卡。

当前可选天赋：

| 天赋 | 最大层数 | 当前效果 |
|---|---:|---|
| 幻影足球 | 3 | 每层在传球时多生成 1 颗幻影足球；多颗幻影足球平分 60 度散射角。 |
| 大脚怪前锋 | 3 | 前锋接球范围每层提高 10%，并同步更新范围指示器。 |
| 额外前锋 | 2 | 每层额外获得 1 个右方前锋球员。 |
| 额外中场 | 2 | 每层额外获得 1 个右方中场球员。 |
| 额外后卫 | 2 | 每层额外获得 1 个右方后卫球员。 |
| 护球立场 | 3 | 己方接球时产生圆形震荡波，推开周围敌方球员。 |
| 传球立场 | 3 | 己方传球时产生圆形震荡波，推开周围敌方球员。 |
| 幻影球员 | 1 | 己方传球后可按 Z/B 在球所在位置生成幻影球员；每次进球后的新一轮最多使用 1 次。 |

香蕉球暂时保留枚举和曲线代码，但当前不在可选天赋池中。

### 4.9 幻影足球规则

获得“幻影足球”后，己方每次传球会生成幻影足球。

当前规则：

- 幻影足球使用独立预制体 `KeepBallPhantomBall.prefab`。
- 每层生成 1 颗幻影足球，最多 3 层。
- 1 层：1 颗，沿传球方向。
- 2 层：2 颗，平分 60 度散射角。
- 3 层：3 颗，平分 60 度散射角。
- 幻影足球不会被己方 hold。
- 幻影足球打进对方球门算有效进球。
- 幻影足球有效进球时不会立刻销毁，会保留给进球慢镜头观看。
- 幻影足球进入自家球门不计分，会直接销毁。
- 幻影足球弹射达到 3 次后销毁。
- 幻影足球命中敌方球员本体后销毁。
- 当前“敌方拦截”只看球员本体半径和幻影足球半径，不再使用控球范围。
- 重新发球或重新开始比赛时，场上残留幻影足球会清空。

当前拦截判定：

```text
敌方球员中心到幻影足球中心的距离 <= 敌方球员身体半径 + 幻影足球半径
```

### 4.10 幻影球员规则

获得“幻影球员”后，蓝方传球后可按 `Z/B` 在球所在位置生成一个幻影球员。

当前规则：

- 幻影球员使用 `KeepBallPhantomPlayer.prefab`。
- 生成后，球会绕着幻影球员运动。
- 按 `空格/A` 后，球会从幻影球员位置踢出。
- 幻影球员踢出球后消失。
- 每次进球后的新一轮最多使用 1 次。
- 传球后如果暂时不召唤，机会会保留到本轮后续蓝方传球窗口。
- 一旦成功召唤，本轮机会清零，直到下一次进球后重新发球才恢复。

### 4.11 立场类天赋

护球立场和传球立场都会产生圆形震荡波。

当前规则：

- 护球立场：己方接球时触发。
- 传球立场：己方传球时触发。
- 震荡波视觉通过预制体实现。
- 蓝方使用 `KeepBallBlueShockwave.prefab`。
- 红方使用 `KeepBallRedShockwave.prefab`。
- 击退不是瞬间传送，而是通过 `PlayerAgent.KnockbackTo` 做短时间 Lerp。
- 击退期间球员不会执行普通移动。

击退距离配置：

```csharp
shieldFieldPushDistance
passFieldPushDistance
shockwavePushDuration
```

## 5. 预制体资源

当前 Resources 预制体目录：

```text
Assets/Resources/KeepBallMoving/
```

当前预制体：

```text
KeepBallPlayer.prefab
KeepBallPlayer1.prefab
KeepBallBall.prefab
KeepBallPhantomBall.prefab
KeepBallBlueShockwave.prefab
KeepBallRedShockwave.prefab
KeepBallPhantomPlayer.prefab
KeepBallTalentBadge.prefab
GoalEffect.prefab
```

用途：

- `KeepBallPlayer.prefab`：蓝方默认球员预制体。
- `KeepBallPlayer1.prefab`：红方默认球员预制体，可用于挂不同 Animator。
- `KeepBallBall.prefab`：普通足球预制体。
- `KeepBallPhantomBall.prefab`：幻影足球预制体。
- `KeepBallBlueShockwave.prefab`：蓝方立场特效。
- `KeepBallRedShockwave.prefab`：红方立场特效。
- `KeepBallPhantomPlayer.prefab`：幻影球员预制体。
- `KeepBallTalentBadge.prefab`：左右下角天赋条目 UI 预制体。
- `GoalEffect.prefab`：进球瞬间生成在破门位置的世界特效。

如果 Inspector 中没有手动指定，`KeepBallGameManager` 会从 Resources 自动加载默认预制体。

## 6. 常用调参位置

### 6.0 模式与 PVP 输入

```csharp
matchMode
redPrimaryKey
redAlternatePrimaryKey
redPhantomKey
redAlternatePhantomKey
```

### 6.1 控球与释放

```csharp
minHoldRadius
holdAngularSpeed
minReleaseSpeed
maxReleaseSpeed
maxChargeTime
maxBallSpeed
```

### 6.2 球员移动

```csharp
blueMoveSpeed
redMoveSpeed
forwardMoveSpeedMultiplier
midfielderMoveSpeedMultiplier
defenderMoveSpeedMultiplier
holderMoveSpeedMultiplier
separationRadius
separationStrength
redKickoffAutoHoldLockout
redAutoHoldDelay
redPassSpeed
redReleaseSpeed
```

### 6.3 球员预制体视觉

```csharp
blueColor
redColor
blueHighlightColor
redHighlightColor
```

这些字段在 `PlayerAgent` 上，建议直接到 `KeepBallPlayer.prefab` 和 `KeepBallPlayer1.prefab` 中调试。

### 6.4 进球演出

```csharp
goalSlowTimeScale
goalCameraMoveDuration
goalCameraReturnDuration
goalDisplayDuration
goalZoomOrthographicSize
goalEffectLifetime
```

### 6.5 红方 AI 强度

```csharp
redAiStrength
redAutoCatchSkill
redDecisionSkill
redPassSkill
redShotSkill
redKickPowerSkill
redMovementSkill
redMaxCatchRadiusPenalty
redMaxReactionDelayPenalty
redMaxHoldDelayJitter
redMaxDecisionMistakeChance
redMaxPassAimError
redMaxShotAimError
redMaxKickSpeedRandomness
```

### 6.6 天赋参数

```csharp
phantomBallSpawnOffset
phantomBallSpeedMultiplier
phantomBallScatterAngle
phantomBallMaxBounces
bigfootForwardCatchBonusPerStack
shieldFieldRadius
shieldFieldPushDistance
passFieldRadius
passFieldPushDistance
shockwavePushDuration
shockwaveEffectDuration
phantomPlayerKickSpeed
phantomPlayerHoldAngularSpeed
phantomPlayerColor
```

### 6.7 预制体字段

```csharp
bluePlayerPrefab
redPlayerPrefab
phantomPlayerPrefab
ballPrefab
phantomBallPrefab
blueShockwavePrefab
redShockwavePrefab
talentBadgePrefab
goalEffectPrefab
```

## 7. 当前验证方式

推荐使用下面的命令验证脚本编译：

```powershell
dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -m:1 -p:BaseIntermediateOutputPath=Temp\ObjVerifySingle\
```

如果成功，应看到：

```text
0 个警告
0 个错误
```

## 8. 当前注意事项

- 三选一天赋界面仍然是 `OnGUI`，左右下角天赋列表已经是 Unity UI 预制体。
- 天赋数据目前仍写在 `KeepBallGameManager` 中，还没有迁移到 `ScriptableObject`。
- 香蕉球代码保留但不在当前天赋池。
- 幻影足球拦截目前是距离检测，不是物理碰撞回调；判断半径已经改为球员本体半径。
- PVP 当前是“玩家控制抓球、蓄力、释放和幻影球员”，红方球员移动仍然是自动跑位。
- PVP 手柄优先使用 New Input System 的 `Gamepad.all + Joystick.all` 隔离；若 New Input System 为空，则使用 Rewired 的 Joystick 列表隔离。
- 旧 Input Manager 只作为最后兜底；PVP 下旧输入按钮按 J1-J8 实际命名槽位分配，PVE 下才读取 Any。
- 当前 README 记录的是 `KeepBallMoving` 原型部分，不覆盖项目里其它系统。

## 9. 后续建议

优先级建议：

- 把临时 `OnGUI` UI 替换成正式 Unity UI 预制体。
- 将天赋数据从硬编码迁移到 `ScriptableObject`。
- 为天赋卡补正式图标资源。
- 为震荡波、幻影足球、幻影球员添加更明显的视觉反馈。
- 继续调红方 AI，保证挑战性和可玩性之间的平衡。
- 后期天赋池不足 3 个时，可以改成显示满层天赋但置灰，或只显示实际可选数量，避免重复卡。
