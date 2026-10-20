# 5단계: 월드 생성 프로토타입 화면

## 구현

- `WorldGenerationPrototype` 씬은 Unity 메뉴 `GaemMeong > World Generation > Build Prototype Scene`에서 다시 생성할 수 있다.
- 월드 렌더링은 `64×64` 청크 단위 `65×65` 격자 메시로 구성한다. 청크마다 월드 절대 좌표를 사용하고, 공유 경계의 정점 높이를 동일하게 계산한다.
- 고도는 정점 Y좌표에 `HeightScale`을 곱한다. 각 타일은 하나의 512×512 디버그 텍스처 픽셀로 표시한다.
- Game 화면의 최소 IMGUI 패널에서 시드, 지형 프리셋, 대륙 프리셋, 디버그 뷰를 바꾸고 재생성할 수 있다. 디버그 뷰는 지형, 고도, 산악 마스크, 대륙 마스크, 경사, 물이다.
- 설정 에셋은 `Assets/GaemMeong/Settings/WorldGenerationPrototypeSettings.asset`이다. 크기/청크/높이 비율은 Inspector에서 바꿀 수 있다.

## 사용법 및 확인할 부분

1. `Assets/GaemMeong/Scenes/WorldGenerationPrototype.unity`를 연다.
2. Play를 누르고 Game 뷰의 패널에서 시드와 두 프리셋을 바꿔 `Regenerate`를 누른다.
3. Scene 뷰에서 메시를 선택해 높이를 돌려 보고, Game 뷰에서 지형 윤곽과 프리셋별 차이를 확인한다.

프리셋 임계값, 육지 면적, 해안선 모양, 높이 대비는 첫 시각 검토 후 조정한다. 현재 작업은 기능 검증용이라 UI와 색상은 임시다.

## 검증

- Unity Editor 스크립트 컴파일 및 프로토타입 씬 저장 성공.
- Unity Edit Mode 테스트 45개 통과.
- 씬 빌더 메뉴는 기존 씬을 덮어쓰기 전에 확인 창을 띄운다. 설정/재질 에셋은 다시 사용한다.
