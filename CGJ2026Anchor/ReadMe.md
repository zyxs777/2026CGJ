# 《别让球停下来》Unity 原型 ReadMe

## 1. 项目目标

《别让球停下来》是一个 Unity 2D 顶视角足球原型。当前目标不是完整足球模拟，而是验证一个快速、可玩的核心循环：球不能轻易停下，玩家通过抓球、蓄力、传球、射门和天赋选择来对抗红方 AI。

核心循环：

```text
球在大球场里持续运动
蓝方和红方球员各自跑位
蓝方由玩家控制抓球和释放
红方由 AI 自动接球、传球和射门
进球后计分并进入三选一天赋
选完天赋后由失球方开球
```

当前优先级：

```text
先保证物理、控球、传球、射门、进球和回合重置可玩
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

### 4.4 蓝方玩家操作

键盘操作：

```text
空格：抓球 / 开始蓄力 / 确认踢出幻影球员持有的球
松开空格：释放足球
R：重置当前回合
N：重新开始比赛并清空天赋
Z：拥有幻影球员天赋后，在当前传球窗口中生成幻影球员
方向键左右：三选一天赋界面切换选择
鼠标点击：选择天赋
```

手柄操作：

```text
A：抓球 / 蓄力 / 释放 / 确认选择天赋
B：召唤幻影球员
左摇杆左右：切换天赋选择
右摇杆左右：如果项目配置了 RightStickHorizontal，也可切换天赋选择
```

### 4.5 红方 AI

红方会自动：

- 根据控制范围接球。
- 根据职责选择传球或射门。
- 带有可调 AI 强度参数。
- 传球和射门不再绝对精准，会受到误差、反应、决策和力度随机影响。

相关参数在 `KeepBallGameManager` 的 `Red AI Difficulty` 区域。

### 4.6 比分和足球规则

上方 UI 显示蓝红比分。

进球规则：

```text
蓝方进球 -> 红方下一轮开球
红方进球 -> 蓝方下一轮开球
```

每次重新发球时：

- 所有球员回到阵型位置。
- 开球球员固定在本方后卫中路开球点。
- 开球持球期间，开球球员不会被人数变化、阵型重新分布或移动 AI 推走。
- 开完球后，开球球员恢复正常移动。

### 4.7 天赋系统

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

### 4.8 幻影足球规则

获得“幻影足球”后，己方每次传球会生成幻影足球。

当前规则：

- 幻影足球使用独立预制体 `KeepBallPhantomBall.prefab`。
- 每层生成 1 颗幻影足球，最多 3 层。
- 1 层：1 颗，沿传球方向。
- 2 层：2 颗，平分 60 度散射角。
- 3 层：3 颗，平分 60 度散射角。
- 幻影足球不会被己方 hold。
- 幻影足球进球也算有效。
- 幻影足球弹射达到 3 次后销毁。
- 幻影足球命中敌方球员本体后销毁。
- 当前“敌方拦截”只看球员本体半径和幻影足球半径，不再使用控球范围。
- 重新发球或重新开始比赛时，场上残留幻影足球会清空。

当前拦截判定：

```text
敌方球员中心到幻影足球中心的距离 <= 敌方球员身体半径 + 幻影足球半径
```

### 4.9 幻影球员规则

获得“幻影球员”后，蓝方传球后可按 `Z/B` 在球所在位置生成一个幻影球员。

当前规则：

- 幻影球员使用 `KeepBallPhantomPlayer.prefab`。
- 生成后，球会绕着幻影球员运动。
- 按 `空格/A` 后，球会从幻影球员位置踢出。
- 幻影球员踢出球后消失。
- 每次进球后的新一轮最多使用 1 次。
- 传球后如果暂时不召唤，机会会保留到本轮后续蓝方传球窗口。
- 一旦成功召唤，本轮机会清零，直到下一次进球后重新发球才恢复。

### 4.10 立场类天赋

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
KeepBallBall.prefab
KeepBallPhantomBall.prefab
KeepBallBlueShockwave.prefab
KeepBallRedShockwave.prefab
KeepBallPhantomPlayer.prefab
KeepBallTalentBadge.prefab
```

用途：

- `KeepBallPlayer.prefab`：默认球员预制体。
- `KeepBallBall.prefab`：普通足球预制体。
- `KeepBallPhantomBall.prefab`：幻影足球预制体。
- `KeepBallBlueShockwave.prefab`：蓝方立场特效。
- `KeepBallRedShockwave.prefab`：红方立场特效。
- `KeepBallPhantomPlayer.prefab`：幻影球员预制体。
- `KeepBallTalentBadge.prefab`：左右下角天赋条目 UI 预制体。

如果 Inspector 中没有手动指定，`KeepBallGameManager` 会从 Resources 自动加载默认预制体。

## 6. 常用调参位置

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

### 6.3 红方 AI 强度

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

### 6.4 天赋参数

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

### 6.5 预制体字段

```csharp
bluePlayerPrefab
redPlayerPrefab
phantomPlayerPrefab
ballPrefab
phantomBallPrefab
blueShockwavePrefab
redShockwavePrefab
talentBadgePrefab
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
- `RightStickHorizontal` 轴如果项目 Input Manager 中没有配置，会被捕获并静默忽略。
- 当前 README 记录的是 `KeepBallMoving` 原型部分，不覆盖项目里其它系统。

## 9. 后续建议

优先级建议：

- 把临时 `OnGUI` UI 替换成正式 Unity UI 预制体。
- 将天赋数据从硬编码迁移到 `ScriptableObject`。
- 为天赋卡补正式图标资源。
- 为震荡波、幻影足球、幻影球员添加更明显的视觉反馈。
- 继续调红方 AI，保证挑战性和可玩性之间的平衡。
- 后期天赋池不足 3 个时，可以改成显示满层天赋但置灰，或只显示实际可选数量，避免重复卡。
