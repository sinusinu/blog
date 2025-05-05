---
title: "개인적으로 쓰는 Git 단축어 목록"
date: 2025-05-05 17:09:00 +0900
last_modified_at: 2025-05-05 17:09:00 +0900
categories: [ "컴푸터" ]
excerpt: "gs gap gc gu"
---

## 많은 단축어는 좋은 단축어

유튜브에서 Andreas Kling 분이 `git add -p`라는 유용한 명령어를 `gap` 단축어에 걸어놓고 사용하는 모습을 보고 '오 나도 따라해봐야지'한 걸로 시작해서, 지금은 자주 사용하는 여러가지 Git 명령어들을 죄다 단축어로 만들어 사용하고 있다.

아래는 `doskey` 명령을 활용하여 단축어들을 등록하는 Windows 배치 파일이다.

이 배치 파일을 적당한 곳에 저장해두고, 명령 프롬프트가 켜질 때마다 자동으로 실행되도록 레지스트리를 통해 설정해두면 이 단축어들을 어디서든 사용할 수 있게 된다.

자동실행 레지스트리 값 경로는 `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Command Processor\AutoRun`이다.

```batch
@echo off

REM 모든 파일 추적 && patch 모드로 git add 실행
doskey gap=git add -N . $T git add -p

REM add된 파일 커밋, 추가 플래그 받음
doskey gc=git commit $*

REM 레포 상태 (커밋 안 한 변경사항, 헤드 상태 등 확인)
doskey gs=git status

REM 푸시("업"로드), 추가 플래그 받음
doskey gu=git push $*

REM 풀("다"운로드)
doskey gd=git pull

REM 커밋 로그 보기
doskey gl=git log
```

<img src="/blog/assets/images/git-shortcuts/registry.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">자동실행 등록은 이렇게</div>

<img src="/blog/assets/images/git-shortcuts/usage.png" style="display:block; margin-left:auto; margin-right:auto">
<div class="img-cap">편-안</div>

컴퓨터 초기화하려다 한 번 잃어버릴 뻔해서, 분실방지 겸으로 블로그에 올려놓는다.