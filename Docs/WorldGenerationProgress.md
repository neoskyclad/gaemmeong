# 월드 생성기 진행 현황

## 완료

- 0단계: 작업 상태 확인
- 1단계: URP 렌더러 최소 검증
- 2단계: 월드 데이터·좌표·설정
- 3단계: 시드·기본 노이즈·고도 생성 (`WorldGenerationStage3.md`)
- 4단계: 지형/대륙 마스크·외해 연결 판정 (`WorldGenerationStage4.md`)
- 5단계: 청크 메시·디버그 텍스처·Unity 프로토타입 씬 (`WorldGenerationStage5.md`)

## 현재 확인 지점

Unity에서 `Assets/GaemMeong/Scenes/WorldGenerationPrototype.unity`를 열고 Play를 눌러 화면을 확인한다. Game 뷰에서 시드, 지형 프리셋, 대륙 프리셋, 디버그 뷰를 바꿔 재생성할 수 있다.

이번 화면 검토에서 특히 볼 항목은 섬/육지 비율, 해안선 굴곡, 산악 밀도, 높이 대비다. 이는 코드 변경을 계속하기 전에 실제 화면에 맞춰 조정해야 할 아트-기술 파라미터다.

## 검증

- Unity Edit Mode 테스트: 46개 통과, 0개 실패.
- Unity 스크립트 컴파일 및 프로토타입 씬 생성 성공.
