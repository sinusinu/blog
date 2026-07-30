# 시누의 블로그

이 레포지토리는 [시누의 블로그](https://sinusinu.github.io/blog/) 웹사이트 데이터와 시블생키를 포함하고 있습니다.

시누의 블로그 게시글(`posts` 디렉토리)과 스켈레톤 파일들(`skel` 디렉토리)은 CC BY-NC-SA 4.0 라이선스 하에 배포됩니다. 자세한 정보는 `LICENSE.blog` 파일을 참고하세요.

# 시블생키

시블생키<sub>시누의 블로그 생성 키트</sub>는 시누의 블로그 웹사이트를 생성하기 위한 정적 사이트 생성기로, GitHub Actions를 통한 사용을 염두에 두고 개발되었습니다.

Jekyll과 유사하게 YAML front matter + Markdown 파일을 게시글로 읽어들이고, 스켈레톤 파일에 게시글을 결합하여 정적 사이트를 생성합니다.

(주의: YAML front matter의 형식이 Jekyll과 완벽 호환되지는 않습니다.)

완성된 정적 사이트는 `dist` 디렉토리에서 찾을 수 있습니다.

시블생키(`posts`와 `skel`을 제외한 모든 파일)는 GNU GPL v3 라이선스 하에 배포됩니다. 자세한 정보는 `LICENSE.siblsenki` 파일을 참고하세요.
