# Random-Circuit

Unity로 제작한 모바일 3D 장애물 레이스 게임  
플레이마다 서로 다른 맵 조합으로 구성된 랜덤 서킷을 최대한 빠르게 통과하는 것이 게임의 목표입니다.

Single Player와 최대 4인의 Online Multiplayer를 지원합니다.

<img width="1308" height="729" alt="multi" src="https://github.com/user-attachments/assets/8cfa4735-b367-40bf-a271-5fb9cd0fa75b" />

---

## Overview

독립적으로 제작된 **4개의 Map Module 중 3개를 중복 없이 무작위로 선택**하여 하나의 전체적인 맵을 구성합니다.

맵에 따라서 일반적인 이동뿐만 아니라 횡스크롤 점프, 자유 낙하 등 서로 다른 이동 방식을 사용하도록 플레이어 이동 시스템을 설계했습니다.

| 항목 | 내용 |
| --- | --- |
| Engine | Unity 6 |
| Language | C# |
| Platform | Android |
| Development | 개인 프로젝트 |
| Genre | 3D Obstacle Race |
| Networking | Netcode for GameObjects / Multiplayer Services / Relay |
| Multiplayer | Single / 2~4 Players |
| Development Period | 2026.08 |

---

# Gameplay

플레이어는 **이동 / 점프 / 대시 / 상호작용**을 이용해 장애물을 통과합니다.

```text
Game Start
    ↓
Random Map 1
    ↓
Random Map 2
    ↓
Random Map 3
    ↓
Goal
```

## Controls

### PC Test Controls

| Action | Key |
| --- | --- |
| Move | WASD |
| Jump | Space |
| Dash | Left Shift |
| Interact | Control |

실제 모바일 환경에서는 화면의 가상 조이스틱과 UI 버튼을 통해 동일한 기능을 사용할 수 있도록 구성했습니다.

<img width="1299" height="729" alt="UI" src="https://github.com/user-attachments/assets/ae2b34cc-4263-4b59-9960-7cfaa9826f97" />

---

# Map Modules

각 맵은 독립적인 `MapModule`로 제작되어 있으며 `EntrancePoint`와 `ExitPoint`를 기준으로 서로 연결됩니다.

새로운 맵을 추가하더라도 전체 코스 생성 로직을 수정하지 않고 동일한 방식으로 확장할 수 있도록 구성했습니다.

```text
MapModule
├── EntrancePoint
│
│   Gameplay Area
│
└── ExitPoint
```

## 1. Cannon Map

대포와 팬을 활용해 이동하는 맵입니다.

플레이어는 대포와 상호작용을 통해 발사 각도와 게이지를 조절하여 이동할 수 있습니다.

Fan 구간에서는 바람의 방향과 거리에 따라 플레이어에게 힘을 적용합니다.

<img width="1299" height="729" alt="Cannon" src="https://github.com/user-attachments/assets/fa87b41f-c7c9-44f7-b7e4-df64c7a68ee3" />

<img width="1299" height="729" alt="Fan" src="https://github.com/user-attachments/assets/f2db2176-e6e5-4318-a0c8-5fa0f24fcbb9" />

---

## 2. Tile Map

밟으면 떨어지는 타일과 지속적으로 발사되는 Projectile을 피하며 진행하는 맵입니다.

Projectile은 많은 개체가 동시에 생성될 수 있기 때문에 Object Pool과 중앙 업데이트 방식을 적용했습니다.

<img width="1299" height="729" alt="Gameplay" src="https://github.com/user-attachments/assets/ac9f340e-38c6-4769-8c16-c37c45de46fb" />

---

## 3. Rotating Map

회전하는 장애물의 움직임을 피하며 통과하는 맵입니다.

회전 장애물과 충돌하면 장애물의 선형 속도와 회전 속도를 기반으로 Knockback 방향을 계산합니다.

맵 곳곳에 있는 구멍과 상호작용을 통해 구멍 안으로 숨어들어가서 장애물을 피할 수 있습니다.

<img width="1299" height="729" alt="RotatingMap" src="https://github.com/user-attachments/assets/5cd4ad97-76af-468c-bcc4-7592f0711269" />

<img width="1276" height="580" alt="image" src="https://github.com/user-attachments/assets/cb1d3dbf-81f7-4e2c-88e0-224f1dcd39ae" />


---

## 4. JumpDrop Map

하나의 맵 안에서 서로 다른 두 가지 이동 방식을 경험할 수 있도록 구성했습니다.

### JumpKing Section

카메라가 Side View로 변경되며 좌우 이동과 점프를 이용해 위쪽으로 올라가야 합니다.

벽과 충돌하면 진행 방향이 반전됩니다.

### Dropper Section

공중에서 방향을 조작하여 장애물을 피해야 합니다.

<img width="1299" height="729" alt="JumpMap" src="https://github.com/user-attachments/assets/f8cc5bf3-f005-4531-82bb-891f1bb4a47e" />

<img width="1299" height="729" alt="DropMap" src="https://github.com/user-attachments/assets/8aae51eb-6199-4201-ba21-522c0e5272c4" />


---

# Random Map System

게임 시작 시 4개의 Map Module 중 3개를 중복 없이 무작위로 선택해 코스를 구성합니다.

```text
Available Maps

Cannon
Tile
Rotating
UpDown

        ↓ Random Selection

Example

Tile
  ↓
UpDown
  ↓
Cannon
```

각 Map Module의 `ExitPoint` 위치에 다음 맵의 `EntrancePoint`가 위치하도록 생성합니다.
이 구조 덕분에 새로운 맵을 추가할 때 기존 코스 생성 로직을 크게 수정하지 않고 확장할 수 있습니다.

---

# Player Movement Architecture

맵마다 요구되는 이동 방식이 다르기 때문에 모든 이동 로직을 하나의 `PlayerMovement` 안에서 조건문으로 처리하지 않고 **Strategy Pattern**을 사용해 이동 방식을 분리했습니다.

```text
PlayerMovement
      │
      │  selects strategy
      ↓
IPlayerMovementStrategy
      │
      ├── NormalMovementStrategy
      │
      ├── JumpKingMovementStrategy
      │
      └── DropperMovementStrategy
```

### NormalMovementStrategy

기본적인 3D 이동을 담당합니다.

- Camera Relative Movement
- Jump
- Coyote Time
- Jump Buffer
- Dash
- Gravity

### JumpKingMovementStrategy

횡스크롤 형태의 이동을 담당합니다.

- 좌우 이동
- 충전 점프
- 공중 조작 제한
- 벽 충돌 시 반동

### DropperMovementStrategy

낙하 구간의 이동을 담당합니다.

- 공중 XZ 이동
- 전용 Gravity
- 최대 낙하 속도 제한
- Jump / Dash 비활성화

각각의 이동 방식이 서로 영향을 주지 않도록 책임을 분리했습니다.

---

# Interaction System

대포처럼 플레이어가 직접 사용할 수 있는 오브젝트는 공통 `Interactable` 구조를 기반으로 처리했습니다.

플레이어 주변의 상호작용 대상을 탐색할 때 `Physics.OverlapSphereNonAlloc`과 재사용 가능한 Collider 배열을 사용해 불필요한 메모리 할당을 줄였습니다.

이를 통해 상호작용 시스템을 새로운 오브젝트에도 확장할 수 있도록 구성했습니다.

<img width="1299" height="729" alt="Interaction" src="https://github.com/user-attachments/assets/3ea53153-f5f7-4ad9-96e0-4405e83ffad9" />

---

# Multiplayer

기존 Single Player 게임을 기반으로 **최대 4인의 Online Multiplayer**를 지원하도록 확장했습니다.

Unity Netcode for GameObjects와 Multiplayer Services / Relay를 사용했으며, Host가 Room을 생성하면 Join Code를 통해 다른 플레이어가 참가할 수 있습니다.

```text
Create / Join
    ↓
Room
    ↓
Host Play
    ↓
Relay Connection
    ↓
Gameplay
```

<img width="1308" height="729" alt="join" src="https://github.com/user-attachments/assets/0d8e3ea0-f5c6-4209-9e05-68a134630c08" />


---

## Network Architecture

Multiplayer로 확장하면서 모든 오브젝트에 `NetworkObject`와 `NetworkTransform`을 적용하지 않고, 게임 요소의 역할에 따라 동기화 방식을 구분했습니다.

### Server Authority

여러 플레이어가 동시에 접근했을 때 하나의 결과를 결정해야 하는 상태는 서버에서 관리합니다.

```text
Game Flow
Goal / Ranking
Retry Ready
Cannon / HideHole
Falling Tile
```

예를 들어 Cannon은 Client가 직접 점유 상태를 변경하지 않고 서버에 사용 요청을 보냅니다. 서버가 현재 상태를 확인한 뒤 한 명의 플레이어에게만 사용을 승인합니다.

### ServerTime Local Simulation

시간을 기준으로 동일하게 재현할 수 있는 움직임은 Transform을 지속적으로 전송하지 않고 `ServerTime`을 기준으로 각 Client에서 계산합니다.

```text
Rotating Bar
Fan
Projectile Cannon
Cannon Projectile
```

이를 통해 움직이는 모든 장애물에 NetworkTransform을 적용하지 않고도 동일한 플레이 상태를 유지하도록 구성했습니다.

### Owner Local

플레이어 개인에게 필요한 기능은 각 Player의 Owner가 직접 처리합니다.

```text
Input
Movement
Camera
UI
Physical Response
```

Player Transform과 Animator 상태만 필요한 방식으로 다른 Client에 동기화합니다.

---

## Disconnect Handling

Client가 이탈해도 2명 이상이 남아 있다면 현재 게임을 계속 진행하며, Finish / Retry 조건은 현재 연결된 인원을 기준으로 다시 계산합니다.

Host가 이탈하거나 Host만 남게 된 경우에는 Session을 종료하고 Main Menu로 복귀하도록 구성했습니다.

---

# Optimization

기능 구현 이후 여러 시스템의 업데이트 방식과 Physics 사용을 정리했습니다.

## Projectile Pool

초기 구현에서는 각각의 Projectile이 자신의 Update()에서 이동과 수명을 처리했습니다.

Projectile이 많아질수록 MonoBehaviour.Update()가 많이 호출되어서 하나의 `ProjectilePool`에서 관리하도록 변경했습니다.

```text
             Projectile.Tick()
            ↗
ProjectilePool
            ↘
             Projectile.Tick()
            ↘
             Projectile.Tick()
```

ProjectilePool.Update()가 현재 활성화된 Projectile만 순회하며 이동과 Lifetime을 처리합니다.

Projectile 생성과 제거에는 Unity의 `ObjectPool<T>`를 사용하여 반복적인 Instantiate / Destroy를 줄였습니다.

또한 활성 Projectile 목록에서 제거할 때 리스트 전체를 이동시키지 않도록 마지막 원소와 교환하는 방식으로 O(1)의 시간복잡도로 제거가 가능하도록 구성했습니다.

---

## Map Activity System

현재 플레이어가 위치하지 않은 맵의 일부 시스템이 계속 동작하지 않도록 `MapActivity`를 사용합니다.

처음에는 각 오브젝트가 매 프레임 Map 상태를 확인하는 방식이었지만 이후 상태 변화가 발생했을 때만 전달하도록 이벤트 기반으로 변경했습니다.

예를 들어 Cannon 맵이 비활성화되면 필요 없는 Fan 회전 및 VFX 등의 동작도 중지됩니다.

---

## Event Based UI

Movement Mode와 같이 값이 자주 변경되지 않는 상태는 UI에서 매 프레임 확인하지 않도록 이벤트를 사용했습니다.

반대로 Dash Cooldown처럼 매 프레임 화면에 변화가 필요한 UI만 Update()를 유지했습니다.

이를 통해 지속적으로 갱신해야 하는 값과 상태가 변경될 때만 갱신하면 되는 값을 구분했습니다.

---

## Physics

불필요한 Physics 계산을 줄이기 위해 Layer Collision Matrix를 구성했습니다.

예를 들어 Projectile끼리의 충돌처럼 게임플레이에 필요하지 않은 조합은 Physics Layer 단계에서 충돌하지 않도록 설정했습니다.

<img width="310" height="271" alt="image" src="https://github.com/user-attachments/assets/ad293038-b7c3-48b8-b5ea-218c09d718a8" />

---

# Camera System

카메라는 Cinemachine을 기반으로 구성했습니다.

일반적인 맵에서는 플레이어를 따라가는 Third Person Camera를 사용하며 JumpKing 구간에서는 Side View Camera로 전환됩니다.

```text
Normal / Dropper
        ↓
Third Person Camera


JumpKing
        ↓
Side View Camera
```

모바일에서는 UI Drag를 통해 카메라를 회전시킬 수 있도록 구현했습니다.

또한 맵의 규모에 따라 Camera Far Clip 값을 변경하여 현재 플레이에 필요하지 않은 먼 영역까지 렌더링하지 않도록 구성했습니다.

<img width="1299" height="729" alt="sideview" src="https://github.com/user-attachments/assets/301c5ea5-9aff-411a-a018-821fec396822" />

---

# UI & Game Flow

게임은 다음과 같은 흐름으로 구성됩니다.

```text
Main Menu
    │
    ├── Single
    │      ↓
    │   Gameplay
    │
    └── Multi
           ↓
       Create / Join
           ↓
          Room
           ↓
       Host Play
           ↓
        Gameplay

Gameplay
    ↓
Countdown
3 → 2 → 1 → GO
    ↓
Race
    ↓
Goal / Ranking
    ↓
Clear
    ↓
Retry / Exit
```

<img width="1383" height="771" alt="image" src="https://github.com/user-attachments/assets/fba784b5-eed0-4a3a-8a46-c3062b8dbbe7" />

---

# Records

플레이 결과는 `PlayerPrefs`를 이용해 로컬에 저장합니다.

저장되는 기록은 다음과 같습니다.

```text
Best Time
Total Clears
Total Respawns
```

Main Menu의 Records 메뉴에서 언제든 확인할 수 있습니다.

<img width="1376" height="771" alt="image" src="https://github.com/user-attachments/assets/d7128c0a-f8e0-4efb-a323-e8e0faa42129" />


---

# Audio & Settings

BGM과 SFX는 하나의 `AudioMixer`를 통해 관리합니다.

```text
GameAudioMixer

Master
├── BGM
└── SFX
```

각 AudioSource가 BGM 또는 SFX Mixer Group으로 출력되도록 구성하여 개별 오브젝트가 자체 AudioSource를 사용하더라도 전체 볼륨을 한 번에 제어할 수 있습니다.

Settings 메뉴에서는 다음 두 가지 볼륨을 조절할 수 있습니다.

```text
BGM Volume
SFX Volume
```

설정값은 `PlayerPrefs`에 저장되어 게임을 다시 실행해도 유지됩니다.

<img width="1383" height="758" alt="image" src="https://github.com/user-attachments/assets/8363017f-bc73-450b-b03d-cc6ad67e8496" />

---

# Project Structure

주요 Script는 기능 단위로 분리했습니다.

```text
Scripts
├── Audio
├── Camera
├── GamePlay
├── Interaction
├── MainMenu
├── Maps
├── Network
├── Player
└── UI
```

---

# 프로젝트를 통해 배운점

이 프로젝트에서는 단순히 장애물을 구현하는 것뿐만 아니라 **기능을 추가하기 쉬운 구조와 최적화 방법**을 고민했습니다.

특히 다음 내용을 직접 구현하고 개선했습니다.

- 서로 다른 이동 규칙을 Strategy Pattern으로 분리
- Entrance / Exit 기반의 재사용 가능한 Map Module 구조
- Object Pool을 이용한 Projectile 재사용
- 다수의 Projectile Update를 하나의 Manager Update로 통합
- Cinemachine을 이용한 게임 구간별 Camera Mode 전환
- 모바일 터치 입력 및 UI 구성
- AudioMixer 기반 BGM / SFX 관리

Single Player 완성 이후 Multiplayer로 확장하면서는 **모든 게임 요소를 단순히 네트워크 동기화하는 것이 아니라 각 시스템의 책임에 따라 동기화 방식을 구분하는 것**을 중점적으로 고민했습니다.

- 경쟁 상태가 필요한 기능은 Server Authority로 관리
- 시간으로 재현 가능한 움직임은 ServerTime 기반 Local Simulation 적용
- Player Input / Camera / UI는 Owner Local로 분리
- Runtime Spawn Player에 맞춰 Camera / UI Reference를 Runtime Binding 방식으로 변경
- Host / Client Disconnect 상황에 따른 Session 종료 및 Game Flow 처리
- Multiplayer Services와 Relay를 이용한 Android 기기 간 Online Multiplayer 구현

이를 통해 Single Player 구조를 유지하면서도 필요한 부분에만 Network Responsibility를 추가하는 방식으로 Multiplayer 시스템을 확장했습니다.
---
