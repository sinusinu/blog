---
title: "이상한 모니터 이야기"
date: 2024-03-09 15:28:00 +0900
last_modified_at: 2024-03-14 09:46:00 +0900
categories: [ "컴푸터" ]
excerpt: "엣지 케이스의 고통"
---

## 모니터는 크기만 크면?

연초에 메인으로 쓰던 23인치 모니터를 서브로 돌리고, 대신 어디서 주워온 오래된 32인치 삼성 LCD TV를 새로운 메인 모니터로 들였다. 처음에는 크게 늘어난 화면 크기에만 감탄하며 만족스럽게 사용했지만, 하루하루 사용할수록 화면이 어딘가 이상하다는 생각이 들기 시작했다. 오래된 TV라 화질 저하가 있는 것이 아닐까 생각도 해봤지만, TV가 오래 되면 화면의 색이 바래든 백라이트가 나가든 하지 이렇게 특이하게 이상해진다는 소리는 어디서도 들어본 적이 없었다.

그러던 어느 날, 갑자기 설마? 하는 한 가지 생각이 머릿속을 스쳤고, 확인해본 결과 그 생각은 불행히도 사실로 드러났다.

<img src="/assets/images/cleartype-and-bgr/ohno.webp" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">조졌네</div>

그렇다. 이 TV는 BGR 서브픽셀 레이아웃을 가지고 있는 TV였던 것이다.

## 서브픽셀 레이아웃

꽤 오래 전 OLED TV가 처음 시장에 등장했을 때 일었던 WRGB 논란, 또는 삼성 스마트폰 초창기의 AMOLED 디스플레이 가독성 논란을 들어봤다면 이 단어도 거기서 들어본 적이 있을 것이다.

LCD 또는 LED 디스플레이에서 하나의 픽셀은 빨강, 초록, 파랑의 세 가지 색으로 이루어져 있으며, 이 세 가지 색의 밝기를 조합해 검은색부터 흰색까지의 색을 만들어낸다.

이 세 가지 색의 밝기는 다른 색의 밝기와 완전히 별도로 조작될 수 있어야 하기 때문에, 각 픽셀마다 개별 색을 내는 부품이 따로따로 심어져 있으며, 이 개별 색을 나타내는 픽셀을 서브픽셀이라고 하며, 이러한 서브픽셀을 픽셀 안에 배치하는 방법을 서브픽셀 레이아웃이라고 부른다.

<img src="/assets/images/cleartype-and-bgr/LCD_RGB.jpg" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">일반적인 RGB 서브픽셀 레이아웃 / Luís Flávio Loureiro dos Santos, CC BY 3.0 DEED</div>

가장 일반적으로 쓰이는 서브픽셀 레이아웃은 RGB로, 1/3 너비의 서브픽셀을 빨강, 초록, 파랑 순서대로 배치하는 것을 의미한다.

이 외에도 내 TV같이 (원가절감에 수월하다는 듯한)BGR 레이아웃, 그리고 OLED TV 당시 논란이 일었던 (화면 밝기를 올려준다는 듯한)WRGB 레이아웃 등 다양한 레이아웃이 존재하지만, 서브픽셀 레이아웃의 de facto standard는 RGB이며, 특히 컴퓨터 모니터용으로는 RGB 레이아웃을 사용하는 것이 매우 권장된다.

그 이유는 **컴퓨터 화면은 서브픽셀 안티에일리어싱을 사용하기 때문이다.**

## 쭈글쭈글한 글자는 물에 넣으면 펴지나요?

지금의 레티나 디스플레이를 탑재한 Mac이나 4K 디스플레이를 사용하는 Windows에서는 조금 더 근본적으로 해결된 문제이긴 하지만, 초창기의 저해상도 LCD에서 벡터 폰트를 깔끔하게 표현하는 것은 몹시 어려운 일이었다.

1980년대 말에 개발되어 지금까지도 사실상 업계 표준으로 사용되고 있는 폰트 포맷인 TrueType(과 현재 그 대안으로 쓰이고 있는 OpenType) 폰트는 각 글자의 모양을 벡터 데이터로 저장한다. 정해진 크기의 픽셀 배열로 이루어진 비트맵 폰트와 달리, 무한한 해상도의 좌표계 위의 점과 선으로 이루어진 벡터 그래픽은 아무리 크기를 키워도 가장 매끄러운 모습으로 재구축이 가능하기 때문에, 컴퓨터 화면보다 높은 해상도의 프린터[^1]에서 인쇄될 때도 디테일을 최대한 살려야 하는 글꼴의 형태를 저장하기에 적절한 선택이었다.

문제는 이러한 벡터 폰트는 고해상도 매체에서 큰 크기로 사용될 때(예: 인쇄)는 아주 깔끔하게 보이지만, 저해상도 매체에서 작은 크기로 사용될 때(예: 컴퓨터 화면)는 사정없이 망가진다는 것이었다.

해상력의 부족으로, 벡터 폰트가 일반적으로 가지는 세밀한 디테일을 표현할 자리가 말 그대로 존재하지 않아 모두 소실되면서 글꼴이 제작자의 의도와 다르게 표현되는 것이다.

아래 그림처럼 벡터 폰트를 너무 낮은 해상도의 화면에 래스터화했을 때를 예시로 들면, 세리프 글꼴이 가지는 삐침 등의 디테일이 사라지는 것은 기본이고, 극단적인 경우 복잡한 글자는 아예 못 알아보게 뭉개져버리는 경우도 있다.

<img src="/assets/images/cleartype-and-bgr/rasterization.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">벡터 폰트의 래스터화 (사용된 알고리즘: 시누 눈깔과 그림판)</div>

이러한 문제 때문에 운영체제들은 TrueType 폰트를 지원하면서도 시스템 폰트로는 꽤 오랜 기간동안 저해상도 화면에서 높은 가독성을 띄는 비트맵 폰트[^2]만을 사용했으며, 워드프로세서 등은 인쇄했을 때 깔끔하게 나오는 폰트를 화면에는 우둘투둘하게 그려내곤 했다.

<img src="/assets/images/cleartype-and-bgr/smj.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">나는 그래도 신명조 폰트는 싫더라</div>

그리고 이런 상황에서 튀어나온, 폰트 렌더링 해상도를 비약적으로 높일 수 있는 창의적인 솔루션 중 하나가 바로 이 글의 주제인 서브픽셀 안티에일리어싱 되시겠다.

## 서브픽셀 안티에일리어싱

서브픽셀 안티에일리어싱은 말 그대로, 글꼴의 안티에일리어싱을 서브픽셀 수준에서 수행하는 것이다.

폰트 안티에일리어싱은 기술적으로 안 될 건 없었지만 어차피 저해상도 화면에서는 글자를 더욱 흐리멍텅하게만 만들어서 별로 쓰이지 않는 기술이었는데, 이를 서브픽셀 수준에서 수행하면 안티에일리어싱을 수행하는 화면의 픽셀 수, 즉 해상도가 3배로 늘어나 훨씬 더 미려한 결과물을 얻을 수 있었던 것이다.

<img src="/assets/images/cleartype-and-bgr/subpixel-1.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">픽셀이 3배!</div>

이 방식의 문제점은, 서브픽셀을 조작하기 위해서는 어쩔 수 없이 색을 사용해야 한다는 것이다. 설령 사용자가 원한 것이 검은 선이더라도, 이를 서브픽셀 수준으로 래스터화하려면 어쩔 수 없이 테두리에 색이 입혀지게 되는 것이다. 이 알록달록한 테두리는 보통 1픽셀 두께이기 때문에 적당한 해상도의 화면에서 대충 볼 때는 눈에 잘 띄지 않지만, 이 방식으로 그려진 글자를 글자 크기를 키우지 않고 스크린샷을 캡쳐해 늘린다던가, 혹은 픽셀이 보일 정도로 극단적인 저해상도 환경에서는 어쩔 수 없이 두드러지게 된다.

<img src="/assets/images/cleartype-and-bgr/subpixel-2.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">제가 그은 검은색 선이 왜 알록달록하죠?</div>

<img src="/assets/images/cleartype-and-bgr/cleartype-scaled.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">그림판 만화에서 종종 보이는 늘어난 ClearType의 아티팩트.<br>자세히 보면 위와 같은 구조의 색 배치를 볼 수 있다.</div>

그리고 이 방식의 또 다른 문제점은...바로 RGB 서브픽셀 레이아웃을 사용하지 않는 모니터에서는 결과물이 엉망진창으로 보인다는 것이다.

<img src="/assets/images/cleartype-and-bgr/subpixel-3.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">RGB 서브픽셀 안티에일리어싱된 선을 RGB/BGR 화면에 띄웠을 때.<br>위: 이미지로 저장된 선 / 아래 왼쪽: RGB / 아래 오른쪽: BGR</div>

<img src="/assets/images/cleartype-and-bgr/rgb-on-bgr.jpg" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">진짜 이렇게 보인다...</div>

다행히도 대부분의 운영체제는 서브픽셀 안티에일리어싱을 수행할 서브픽셀 레이아웃을 변경할 수 있으며, Windows에서는 "ClearType 텍스트 튜너"를 통해 서브픽셀 안티에일리어싱을 RGB 기준으로 할 것인지 BGR 기준으로 할 것인지 설정할 수 있다.

<img src="/assets/images/cleartype-and-bgr/cleartype-adjust.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">구원의 손길</div>

하지만...

## 내가 찍은 스크린샷은?

이제 내 컴퓨터에서 렌더되는 글자는 BGR 화면에 맞게 그려지지만, 종종 인터넷에서 남들이 찍은 RGB 서브픽셀 안티에일리어싱된 스크린샷이 보이면 어쩔 수 없이 글자가 번져보였는데, 그걸 보다 보니 그러면 내가 찍은 스크린샷은? 하는 생각이 들었다. 내가 찍은 BGR 서브픽셀 안티에일리어싱된 스크린샷들은 인터넷에 올라가서 대다수의 RGB 모니터에서 이상하게 보일 것이 뻔한데, 이걸 이제 나는 정상적으로 보인다 하고 오케이 하고 넘어갈 일인가?

고심 끝에 그냥 내 모니터에서는 서브픽셀 안티에일리어싱을 포기하고, 흑백 안티에일리어싱을 사용하기로 했다.

어차피 내 막눈으로는 흑백 안티에일리어싱된 글자도 그렇게까지 흐릿하게 보이진 않고, RGB든 BGR이든 32인치 1080p는 개별 픽셀이 보이기 시작하는 정도의 낮은 해상도다 보니, 글자의 해상도보다는 알록달록한 테두리가 더 눈에 띄기 시작한 참이었다.

Windows는 ClearType이라는 자체적인 서브픽셀 안티에일리어싱만 지원하며 흑백 안티에일리어싱을 지원하지 않지만, MacType이라는 프로그램을 통해 흑백 안티에일리어싱을 강제로 주입할 수 있다. 흑백 안티에일리어싱과 폰트 힌팅은 잘 어울리지 않으므로 MacType에서 폰트 힌팅 또한 최소화해주면, 프로그램 이름처럼 Mac과 유사한 모양의 글꼴을 얻을 수 있다.

<img src="/assets/images/cleartype-and-bgr/mactype-onoff.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">위: Windows 기본 ClearType (BGR)<br>아래: MacType 흑백 안티에일리어싱, 힌트 사용 안 함</div>

이렇게 사용한 지 약 한 달 쯤 되어가는데, 이것도 눈에 익으니 꽤 나쁘지 않은 것 같다.

<img src="/assets/images/cleartype-and-bgr/cope.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">나쁘지 않다...리얼루...</div>

## 결론

BGR TV는 모니터로 쓰지 말자.

## 주석

[^1]: 직접적인 비교는 안 되지만 대충 비슷한 단위를 대고 보자면 최신 스마트폰 중에서 손에 꼽을 정도의 고밀도 화면을 탑재한 삼성 갤럭시 S24 울트라의 화면 해상도는 508ppi인데, 같은 삼성의 10만원짜리 싸구려 레이저 프린터의 인쇄 해상도는 1200dpi이다.

[^2]: Windows의 굴림/돋움/바탕/궁서/Times New Roman/Tahoma 등이 이러한 폰트에 해당되며, 이들 폰트는 기본적으로는 벡터 폰트이나 특정 크기(8, 9, 10...)로 출력 시 비트맵 데이터를 대신 사용하게 되어 있다.