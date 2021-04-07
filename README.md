Box Smasher
====

1. [만드는 이유](#만드는-이유)
2. [제공하는 것](#제공하는-것)
3. [개발철학](#개발철학)
4. [목표 사양](#목표-사양)
5. [참가자](#참가자)

# 만드는 이유

스마일게이트 인디게임 창작 공모전 제출용으로 개발중입니다. 


# 게임 설명

물리엔진 기반 탄막 플랫포머 액션 게임입니다.

사각형 플레이어가 탄막을 피하면서 공중에서 힘을 모아 돌진해 부딪히는 방식으로 적을 무찌르는 게임입니다.
스테이지와 보스전을 번갈아 등장시키는 방식으로 개발하고자 합니다.

플레이어의 현재 최대 체력은 3이며, 적에게 한 번 부딪히거나 탄막에 맞을 때마다 체력이 1 줄어들며 잠깐의 무적 시간을 가집니다.
돌진 공격 중일때는 적에게 부딪혀도 데미지를 입지 않지만, 공격이 끝난 후 체공 도중에 적에게 부딪히면 피격된 것으로 판정됩니다.

스테이지에서는 플레이어의 진행상황을 떨어뜨리는 공격이 주로 이루어지고,
보스전에서는 플레이어가 탄막을 피하며 패턴이 단순해진 순간에 보스를 공격해 무찔러야 합니다.

보스에게 들어가는 데미지는 플레이어의 회전속도와 운동속도를 기반으로 계산되며, 최소 데미지가 설정되어 있습니다.


# 개발철학

게임 전반적으로 플레이어가 다양한 기능을 응용하며 플레이할 수 있도록 개발합시다.

공격을 준비하는 중에 체공 시간이 길어지는 것을 이용해 탄막을 피하는 것처럼
공격 준비와 돌진 공격, 점프같은 기본적인 기능을 응용해서 플레이한다면 충분히 끝까지 클리어 가능한 게임을 목표로 합시다.

스테이지에서는 플레이어가 공격과 점프를 이용해 장애물을 헤치고 보스 전투 지역까지 나아가는 것을 주 목표로 둡니다.
따라서 플레이어에게 데미지를 주기보다는 진행을 방해하는 방식의 기믹을 주로 사용하고자 합니다.

약간의 퍼즐 게임같은 요소를 넣어도 좋지만, 가능하면 순발력이 필요한 구간을 같이 넣는 등 플레이어의 텐션을 유지하도록 합시다.

스테이지의 개수를 2~3개까지 개발하게 되면
스테이지마다 보스와 스테이지 전체의 컨셉을 하나씩 정해서 개발해도 재밌을 거 같습니다.

보스들은 탄막을 이용한 공격과 지형지물을 이용한 공격, 그리고 물리 엔진을 이용한 공격 총 세 가지를 주로 이용합니다.
보스의 공격패턴은 가능한 한 일정하게 반복되도록 하되, 랜덤으로 나오는 패턴이나 체력이 낮아지면 추가로 나오는 패턴이 있으면 좋겠습니다.

보스전은 이러한 보스의 공격을 피하고 보스의 공격패턴이 느슨해진 틈을 타 플레이어가 공격하는 방식으로 진행하고자 합니다.


# 목표 사양

화면 크기 : 1920x1080 (16:9)


# 참가자
HookSSi

ctw0727
