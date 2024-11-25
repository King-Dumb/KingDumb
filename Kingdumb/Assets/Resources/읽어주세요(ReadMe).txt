오브젝트 풀이 추가되면서 프리팹 경로에 규칙이 생겼습니다. 이를 준수해주세요

1. 프리팹 중복 이름 짓지 않기

- PhotonNetwork.Instantiate() 로 생성하는 객체는  Resources/Photon에 집어넣기
- 오브젝트 풀로 관리할 객체는 Resources/ObjectPool에 집어넣기
- 그 외의 Resources.Load<>가 필요없는 프리팹은는 Prefabs/ 폴더에 집어넣기


