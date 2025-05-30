---
title: "우당탕탕 5만원짜리 노트북에서 리눅스 굴리기"
date: 2024-05-13 12:37:00 +0900
last_modified_at: 2024-05-28 18:11:00 +0900
categories: [ "컴푸터" ]
excerpt: "아톰 고생은 사서도 한다"
---

## 우리 만남은

약 1년 전, 당근을 뒤적거리던 나는 누군가가 단돈 5만원에 올려놓은 한 노트북을 발견했다.

때마침 험하게 굴릴 x86 장비가 하나 있었으면 좋겠다고 생각하고 있었던 나는, 아무 생각 없이 그 딜을 덥석 물었다.

노트북을 넘겨주던 아저씨는 대뜸 내게 이런 걸 팔아서 미안하다고 사과를 하셨지만, 나는 '나쁘면 얼마나 나쁘다고' 하는 생각에 괜찮다, 감사하다, 잘 쓰겠다고 연신 감사의 인사를 하고 왔다.

이제 와서 생각해보면, 나더러 지금 이 노트북을 다시 팔라고 하면 아마 팔지 못할 것 같다.

이 노트북이 좋아서가 아니라, 그 때 그 아저씨처럼, 이걸 사 갈 사람한테 미안해서.

<img src="/blog/assets/images/linux-on-atom/sb11p.jpg" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">오늘의 주인공, 아이뮤즈 스톰북11 프로</div>

## 리눅스 돌리면 되겠지

지금은 애증을 담아 "쓰레기북"이라고 부르고 있는 이 노트북은 인텔 아톰 x5-Z8350 CPU를 탑재한 노트북이다.

이 아톰과, 셀러론 등 상위 제품군 사이에는 아주 크고 중요한 차이점이 한 가지 존재한다.

바로 아톰 CPU는 일반 소비자에게 단품으로 파는 물건이 아닌, ARM CPU처럼 임베디드 장치를 만들고자 하는 기업에 칩셋 단위로 판매되는 제품군이라는 것이다.

그렇다보니, 아톰 디바이스들은 그 구조가 우리가 흔히 조립하고 리눅스 깔고 할 수 있는 PC보다는, ARM CPU를 탑재한 안드로이드 태블릿 쪽에 더 가까운 경우가 많다.

즉 아톰 디바이스는 일반적인 PC에서 찾아보기 힘든 비표준 하드웨어 구성으로 떡칠이 되어 있고, 이는 자연스럽게 리눅스와의 호환성 감소로 이어진다.

이런 아톰 디바이스에 리눅스를 설치하는 것은 미친 짓이다.

이 글에서 곧 설명할 문제 해결법들은 "터미널에 이 명령어를 복사 붙여넣기 하고..."같이 친절한 튜토리얼이 아닌, "이 값을 이걸로 설정하면 된다" 정도밖에 안 될 것이다.

만약 이 설명만 봐서는 무엇을 해야 하는지 이해할 수 없다면, 그리고 중간중간 예기치 못하게 튀어나오는 오류를 스스로 때려고칠 수 있는 수준의 리눅스 친숙도가 없다면, 아톰 노트북에 리눅스 설치는 때려치우고 **원래 설치되어있는 윈도우를 계속 사용할 것을 추천한다.**

아주아주 기본적인 용도로는 극단적으로 깎아낸 윈도우 환경도 나쁘진 않다. 나도 한 반 년 정도는 많이 깎아낸 윈도우로 쓰기도 했고.

## 근데 왜 리눅스?

하지만 내가 굳이 쓰레기북에 리눅스를 설치하여 사용하는 이유는 다음과 같다.

### 조금 더 쥐어짜내지는 성능

윈도우는 무거운 OS이다. 물론 요즘은 펜티엄 급까지만 올라가도 윈도우가 "무겁다"고 느낄 이유는 싹 사라지지만, 그 아래에서 SBC급 성능을 쥐고 뒹굴어야 하는 쓰레기북에겐 한 톨의 성능이 아깝다.

리눅스를 설치한다고 해서 아톰이 갑자기 i3급이 되고 그런 건 전혀 아니지만, 눈 뜨고 못 봐줄 처참한 성능이 그럭저럭 봐줄만한 성능이 되어, "이제 좀 컴퓨터 같네"같은 칭찬 아닌 칭찬이 튀어나오는, 딱 그 정도의 성능 향상은 생긴다.

### 조금 더 널널해지는 저장공간

윈도우가 무거운 것은 성능 뿐만이 아니다. 윈도우는 용량도 제법 많이 나간다.

32GB의 eMMC에 10GB의 복구 파티션을 할당해둔 이 미친 노트북은 스톡 구성으로는 켜자마자 디스크 공간 부족 알림이 튀어나오고, 복구 파티션을 날린 뒤에도 뭐 하나 잘못하면 디스크 여유 공간이 1GB씩 날아가는 바람에 윈도우를 사용할 때는 한 자릿수 디스크 여유 공간을 늘 주시하며 두려움에 떨어야 했다.

호환성 문제로 아주 가벼운 리눅스는 설치하지 못했지만, 우분투도 최소 구성으로 설치하면 용량이 윈도우의 절반도 안 나가고, 그리고 윈도우처럼 알 수 없는 무언가가 계속 용량을 잡아먹는 일도 안 생긴다.

이렇게 성능과 용량이 간신히 받쳐지는 선까지 올라와, 윈도우에선 상상도 못 하던 개발 도구 설치 등이 가능해진다. 이 글도 초안은 쓰레기북에 설치한 VS Code로 작성됐다.

## 그래서 무슨 리눅스?

일단 결과적으로는 Xubuntu를 설치하여 사용중이다.

개인적으로 사랑해 마지않는 리눅스 민트를 포함한, 상당수의 소규모 매니악 디스트로들은 와이파이가 안된다던가 화면 밝기를 못 내린다던가 등의 하드웨어 호환성 문제가 심해 모두 포기했다.

커널 버전이 6.x인, 그리고 가능하면 업스트림을 빨리 따라가는 메이저 디스트로를 사용하는 것이 속 편하다. 특히 6.x에서 아톰 호환성 관련 패치가 제법 많이 이루어졌기에, 아톰 디바이스에서는 이를 따라가는 것이 필수적이다.

DE는 선택지가 별로 없다. 일단 그놈은 성능 KDE는 안정성 문제로 바로 아웃이고, 주류에서 크게 벗어나지 않으면서 너무 힙하지 않은 DE는 Xfce밖에 없었다.

여담이지만 이런저런 디스트로를 테스트해볼 때, 설치 USB는 정상적으로 구워졌는데 부트가 안 되는 경우가 종종 있었다.

대충 찾아보니, 아톰 체리트레일 제품군은 CPU는 64비트지만 칩셋에 탑재된 EFI 부트로더가 32비트라 64비트 EFI 바이너리를 실행할 수 없다고 한다. 아이고야.

이 문제는 EFI 셸에서 설치 이미지에 운 좋으면 동봉되어 있는 32비트 부트로더(`bootia32.efi`)나 그럽 등을 직접 실행하는 것으로 우회할 수 있었다.

## 산 넘어 산

Xubuntu가 기본 설치 상태에서 그나마 성능과 호환성이 가장 나은 디스트로이긴 했지만, 그렇다고 노트북을 완벽하게 사용할 수 있는 상태는 아니었다. 오히려 기본 설치 상태에서는 사용이 불가능할 정도로 OS 곳곳이 터져나갔다. 정상 사용을 위해서는 기본 설치 위에 몇 가지 추가적인 설정을 해 줘야 했다.

이렇게 하나하나 고칠거면 다른 디스트로도 똑같이 문제 해결만 하면 쓸 수 있는 것 아닌가라고 하면 사실 그 말이 맞다. 다만 다른 디스트로는 시작부터 되는 게 없어서 막막한 반면 Xubuntu는 그나마 반쯤 돌아가는 상태로 시작하기에 리눅스 광인이 아닌 내가 조금 더 수월하게 문제 해결을 할 수 있었다.

아래는 내가 쓰레기북에서 겪은 몇 가지 문제 증상들과 해당 증상들의 해결법이다.

### 문제: 소리 안 남

처음에는 소리가 안 나서 오디오 드라이버를 못 잡는 건가 싶었는데, 오디오 드라이버는 멀쩡하게 잡혀있었고 **이어폰 잭에 이어폰을 꽂으면 스피커에서 소리가 났다.** 즉 이어폰 잭 연결 감지가 반대로 되고 있었던 것이다.

윈도우에서도 드라이버 설치 없이는 같은 증상이 나타나는 것으로 보아 윈도우에서는 드라이버가 이를 인위적으로 뒤집는 듯 했다.

이건 뭐 내가 오디오 드라이버 코드를 손대서 컴파일할 깜도 안되고 이어폰 잭에 무슨 납땜을 할 수도 없고 해서, 그냥 작은 USB 오디오 카드를 사다가 끼워 쓰고 있었는데, 최근에 훨씬 만족스러운 해결법이 나왔다.

바로 PipeWire 1.0.5에서 오디오 잭 탐지 문제가 해결된 것이다! 더러운 PulseAudio는 날려버리고 해당 버전 이상의 PipeWire를 설치하면 오디오는 정상 작동한다.

### 문제: 소리 프리즈

그런데 이렇게 고쳐진 오디오를 틀어놓고 있다 보면 어느 순간 갑자기 나와야 될 소리 대신 깨지는 듯한 비프음이 튀어나온다.

이는 오디오 드라이버인 SOF가 아톰 칩셋과 호환성이 좋지 않아 일어나는 문제로, 왜인지는 몰라도 SOF 드라이버를 디버그 모드로 구동하면 발생하지 않는다고 한다.

SOF 드라이버를 디버그 모드로 구동하려면 그럽에서 커널 부트 커맨드라인에 `snd_sof.sof_debug=1`을 추가해주면 된다.

### 문제: 노트북 프리즈

~~오디오만 뻗어버리면 섭섭하니까~~ CPU 사용률이 높을 때 랜덤하게 OS가 뻗어버리는 문제가 발생한다. 이 역시 아톰 CPU와 리눅스의 호환성 문제로, 아톰 CPU만의 특수한 저전력 C-state를 리눅스가 이해하지 못하고 뻗어버리는 것이라고 한다.

이는 아톰 CPU가 저전력 모드에 못 들어가게 틀어막아버리는 것으로 해결 가능하다. 커널 부트 커맨드라인에 `intel_idle.max_cstate=1`을 추가해주면 된다.

**주의:** 이 옵션을 사용하면 CPU가 풀 파워를 제외한 그 어떤 C-state에도 들어가지 않는다. 노트북이 항상 풀 파워로 돌아갈 뿐만 아니라, 노트북 뚜껑을 덮으면 들어가지는 절전 모드도 이 C-state에 해당되므로, 뚜껑을 덮어도 노트북이 꺼지지 않고 계속 풀 파워로 돌아가게 된다.<br><br>이 옵션을 사용하게 되면 야외에서 배터리 사용은 그냥 없다고 봐야 한다. 쓰레기북의 경우는 집에서 늘 전원선을 꽂아놓고 사용 중이라 상관없지만...
{: .notice--warning}

### 문제: 각종 하드웨어의 반항

한번씩 쓰레기북을 켜면 Wi-Fi 하드웨어 또는 SD 카드가 아예 감지되지 않는다. 가챠마냥 랜덤 발생하는 이벤트라, 그냥 전부 멀쩡하게 잡힐 때까지 계속 껐다 켰다를 반복하면 된다.

이외에도 부트 도중에 알 수 없는 하드웨어의 반항으로 뻗는 경우가 종종 있으니, 뻗은건지 아닌지 확인하기 위해 Plymouth는 꺼놓는 편이 좋다. 커널 부트 커맨드라인에서 `quiet splash`를 지워주면 된다.

또한 왜인지는 몰라도 전원을 켤 때마다 화면 밝기가 최대로 올라간다. 안 내려오는 건 아니라 그냥 부트 끝나고 직접 내리면 되는데, 새벽에 유튜브 볼려고 켤 때마다 눈뽕당하는 건 영 적응이 안 된다.

**업데이트:** 화면 밝기 문제는 X 세션이 시작될 때 화면 밝기를 0으로 떨어트리는 스크립트를 자동 실행시키는 것으로 우회했다! 1~2초간은 섬광이 터지긴 하지만 눈 찌푸리며 Fn+F3 키를 연타할 필요가 없어졌으니 좋다.<br><br>화면 밝기를 0으로 떨어트리려면 `/sys/class/backlight/(컴퓨터마다 다름)/brightness`에 `0`을 써주면 되는데 그러려면 루트 권한이 필요하니, 우선 해당 경로에 `0`을 쓰는 스크립트를 작성하고, sudoers에서 해당 스크립트만 암호 없이 sudo할 수 있게 설정하고, 마지막으로 `~/.config/autostart/`에 해당 스크립트를 `bash -c "sudo ..."`로 실행하는 데스크탑 엔트리를 만들어 넣어주었다.
{: .notice--info}

### 문제: 쌀알만한 인터페이스

쓰레기북은 10.6인치의 작은 크기에 1080p 해상도, 약 200ppi의 고해상도인 디스플레이를 달고 있다. 윈도우는 그걸 알고 자동으로 화면 배율을 150%로 맞춰주기에 편하게 쓸 수 있지만, 아는 사람은 알다시피 리눅스 쪽의 화면 배율 지원은 처참한 상태다.

Wayland에서 배율 지원이 많이 좋아졌다고는 들었는데 메인 컴퓨터에서도 사고치는 사고뭉치가 아톰북에서 멀쩡하게 돌아갈 리는 없다고 보고, X11은 1.5배 확대가 해상도 3배 키우고 2배 다운스케일링이라는 어처구니없는 구현 뿐이니 그냥 포기하고 시스템 해상도를 720p로 낮춰서 쓰고 있다.

문서작업을 할 때는 흐려진 화면이 제법 거슬리지만, 유튜브 볼 때는 별 티 안 난다. 어차피 성능 때문에 유튜브에서 볼 수 있는 영상 해상도도 720p가 한계라 그냥 짝짝 맞는다.

<img src="/blog/assets/images/linux-on-atom/rice.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">해상도를 1080p로 설정한 경우. 쌀알만하다는 건 비유가 아니다</div>

### 성능: 웹 브라우저 추천

유튜브 얘기하는 김에, 웹 브라우저는 [Thorium](https://thorium.rocks/) 사용을 권한다. 크로미움 브라우저에 온갖 자잘한 성능 패치를 갖다붙여 현존 가장 빠른 웹 브라우저를 만들어 놓은 프로그램이다.

비록 크롬이 무난히 돌아가는 환경에서는 별로 체감되지 않는 미세한 성능 개선이지만, 쓰레기북같은 환경에서는 정말 극적인 차이를 만들어낸다.

파폭도 크롬도 유튜브는 영상 720p로 돌리면 죽어나가고 실수로 라이브 방송이라도 틀었다간 채팅창 그리다 말고 노트북을 통째로 얼려버리는 반면, Thorium은 720p까지 무난하게 소화해내고 라이브 방송 채팅도 uBlock으로 사용자 프로필 사진을 안 띄우게 막아두면 아슬아슬하게 소화해낸다.

이 브라우저 덕분에 쓰레기북이 유튜브 머신으로서의 역할은 완벽하게 수행해내고 있다. Alex313031님과 Thorium 기여자 분들께 무한한 감사를...

### 성능: 기타

의외로 성능을 제법 잡아먹는 게 폰트 안티에일리어싱이다. 폰트 안티에일리어싱을 끄면 상당한 성능 향상을 볼 수 있다.

글꼴이 못생겨지는 게 흠이지만 윈도우 PC에서 굴림체 같은 비트맵 폰트를 가져와 붙여두면 나름 레트로 PC같고 좀 볼만해진다.

그리고 끌 수 있는 화려한 효과는 모조리 끄는 게 좋다. 그림자, 애니메이션, 투명 등등 설정 창을 뒤지며 잡히는 대로 모조리 꺼주자.

스냅 지우자. 우분투는 대체 왜 스냅을 미는건지 모르겠다.

## 결론

조금 더 쓸만해진 건 좋지만 솔직히 이렇게까지 해가며 쓰는 보람은 별로 없다.

컴맹은 물론이고 그냥 컴퓨터를 평범하게 쓸 줄 아는 정도의 사람이어도 이런 시스템을 지속적으로 굴리는 건 그냥 불가능하다고 본다.

자랑하려는 건 아니지만 *'나 쯤 되니까 쓸 수나 있는거지'* 싶기도 하고. 이런 물건을 돈 받고 파는 놈들은 제정신인가 싶기도 하고.

굳이 노트북에서 리눅스를 굴리고 싶다면 Lenovo의 ThinkPad 등 유명한 브랜드의 유명한 모델에서 굴리고, 또 가능하면 펜티엄 이하는 피하는 것이 좋겠다.

아무도 모르는 브랜드의 아무도 모르는 아톰 노트북은 "리눅스를 깔아서 더 쓸만하게" 쓸 가치도 없다.

## 여담

찾다보니 알게 된 건데, 이 쓰레기북...아이뮤즈 스톰북11 프로는 그냥 어디 중국 공장에서 찍어내는 제품을 택갈이한 물건이더라.

아이뮤즈 스톰북11 프로와 완전히 똑같이 생긴 노트북이 듣보잡 브랜드를 달고 알리익스프레스에서 팔리고 있었다.

<img src="/blog/assets/images/linux-on-atom/sb11p.jpg" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">아이뮤즈 스톰북11 프로</div>

<img src="/blog/assets/images/linux-on-atom/bdf.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">BDF 초박형 노트북 안드로이드 12 OS 핑크 10.1 인치 2GB RAM 64GB 저장 컴퓨터 신제품<br>알리익스프레스 특전 까리한 핫핑크색</div>

또한 완전히 똑같지는 않지만 키보드, 왼쪽 위 LED 3개 등 유사점이 상당히 많은 제품도 엠피지오에서 하나 찾을 수 있었다.

<img src="/blog/assets/images/linux-on-atom/ares11.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">엠피지오 아레스11</div>

뭐...이게 의미하는 건 이 노트북에 대한 아이뮤즈의 기술 지원은 기대할 수 없다는 것 뿐이겠지만...
