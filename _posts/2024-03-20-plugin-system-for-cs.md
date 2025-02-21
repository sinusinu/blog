---
title: "C#으로 단순한 플러그인 시스템 구축하기"
date: 2024-03-20 22:16:00 +0900
last_modified_at: 2024-04-14 12:56:00 +0900
categories: [ "컴푸터" ]
excerpt: "정말 단순한 예제 포함"
---

## 뭐가 이렇게 복잡해

C#으로 플러그인 시스템을 구축하려고 인터넷에서 관련 자료를 찾아볼랬더니, 쓸데없이 너무 복잡하고 어려운 글들만 잔뜩 나와서 내용을 이해하는 데 좀 고생을 했다.

일단 어떻게 이해는 됐는데, 혹시나 나중에 까먹었을 때 보고 다시 빠르게 이해할 수 있도록, 내가 이해한 플러그인 시스템 구축법을 최대한 단순하게 요약해서 여기 적어놓으려 한다.

다른 사람들에게도 도움이 되면 더 좋을지도...

## 플러그인 시스템의 개요

먼저 내가 원한 플러그인 시스템은 다음과 같다.

- 플러그인 소비자는 인터페이스를 제공한다.
- 플러그인 공급자는 소비자가 제공하는 인터페이스를 상속받는 클래스를 구현한다.
- 플러그인 소비자는 (공급자를 레퍼런스로 직접 포함하지 않고) 런타임 중에 공급자에 구현되어 있는 클래스를 불러와 인터페이스를 통해 사용할 수 있다.

이 방식을 잘 활용하면...

- 플러그인 소비자의 소스 코드를 전혀 손대지 않고도, 새로운 공급자만 개발하여 플러그인 소비자의 기능을 확장시킬 수 있게 된다.
- 플러그인 소비자의 개발자 뿐만 아니라, 누구라도 손쉽게 플러그인 공급자를 개발하여 플러그인 소비자의 기능을 확장할 수 있게 된다.

## 공용 인터페이스 만들기

처음에 이상하게 만들다가 나중에 알게 된 건데, 플러그인 소비자와 플러그인 공급자가 함께 사용할 인터페이스는 같은 [어셈블리](https://learn.microsoft.com/en-us/dotnet/standard/assembly/)에서 와야 한다. 설령 인터페이스의 모양이 완전히 똑같다 하더라도 두 인터페이스가 서로 다른 어셈블리에 있게 되면 완전히 다른 타입으로 취급되므로, 아래와 같은 구조에서는 플러그인 공급자의 클래스를 플러그인 소비자의 인터페이스로 캐스트할 수 없게 된다.

```
PluginProvider
- IPlugin.cs (같은 내용의 파일)
- CatPlugin.cs

PluginConsumer
- IPlugin.cs (같은 내용의 파일)
- Program.cs
```
```csharp
class Program {
    void SomehowGetPlugin() {
        var plugin = (IPlugin)LoadPlugin("provider.dll"); // InvalidCastException!
    }
}
```
<div class="img-cap">이렇게 하면 안 된다!</div>

결국 인터페이스가 한 어셈블리에서만 와야 하는데 공급자에게서는 당연히 올 수 없고, 공급자에게 소비자를 레퍼런스로 추가하게 하는 건 배꼽이 배보다 더 커지는 꼴이 되니, 이래저래 생각해본 결과 다음과 같은 방식이 가장 적절하다고 판단했다.

1. 인터페이스만 제공하는 플러그인 베이스 프로젝트를 별도로 생성한다.
2. 소비자와 공급자 모두 베이스 프로젝트를 레퍼런스로 추가한다.
3. <s>소비자와 공급자가 제발 같은 버전의 베이스 프로젝트를 참조하고 있기를 빈다.</s>

```
PluginBase
- IPlugin.cs

PluginProvider
- CatPlugin.cs

PluginConsumer
- Program.cs
```
```xml
PluginProvider.csproj / PluginConsumer.csproj:
...
  <ItemGroup>
    <ProjectReference Include="path\to\pluginbase\PluginBase.csproj" />
  </ItemGroup>
...
```
<div class="img-cap">이렇게 하면 되긴 된다!</div>

이제 인터페이스를 만들자. 이 인터페이스 파일은 베이스 프로젝트에 들어가며, 소비자와 공급자는 모두 이 베이스 프로젝트를 참조하여야 한다.

```csharp
public interface IPlugin {
    public string Name { get; }

    public void MakeSound();
    public Task GoToSleepAsync();   // no async in interface
}
```

그리고 공급자 프로젝트에 이 베이스 인터페이스를 상속받는 클래스를 만들자. 테스트용으로 2개 만들어봤다.

```csharp
public class CatPlugin : IPlugin {
    public string Name => "Cat";

    public CatPlugin() {}

    public void MakeSound() {
        Console.WriteLine("meow");
    }

    public async Task GoToSleepAsync() {
        await Task.Delay(500);
        Console.WriteLine("The cat has fallen asleep");
    }
}
```
```csharp
public class DogPlugin : IPlugin {
    public string Name => "Dog";

    public DogPlugin() {}

    public void MakeSound() {
        Console.WriteLine("woof woof");
    }

    public async Task GoToSleepAsync() {
        await Task.Delay(500);
        Console.WriteLine("The dog has fallen asleep");
    }
}
```

이제 공급자 프로젝트를 빌드해 플러그인 2개가 담긴 DLL 파일을 뽑아낸 뒤, 소비자 프로젝트의 작업 디렉토리에 `plugins` 폴더를 만들고 그 안에 집어넣자. 인터페이스만 들어있는 베이스 DLL 파일은 어차피 소비자도 갖고 있으므로 갖다둘 필요 없다.

```
C:\...\Provider>dotnet build
msbuild 버전 17.8.3+195e7f5a3(.NET용)
  복원할 프로젝트를 확인하는 중...
  복원할 모든 프로젝트가 최신 상태입니다.
  PluginBase -> C:\...\Base\bin\Debug\net8.0\PluginBase.dll
  PluginProvider -> C:\...\Provider\bin\Debug\net8.0\PluginProvider.dll

빌드했습니다.
    경고 0개
    오류 0개

경과 시간: 00:00:01.39

C:\...\Provider>copy bin\Debug\net8.0\PluginProvider.dll ..\Consumer\plugins
        1개 파일이 복사되었습니다.

C:\...\Provider>
```
<div class="img-cap">비주얼 스튜디오? 그런 걸 왜 써요?</div>

## 플러그인 불러오기

이제 플러그인 소비자에서 방금 만든 플러그인을 불러와보자! async 함수도 테스트할 예정이니 async Main에서부터 시작해보자.

```csharp
public class Program {
    static async Task Main(string[] args) {
        Console.WriteLine("Loading plugins...");

        List<IPlugin> loadedPlugins = new();

        // TODO: load plugins

        Console.WriteLine($"Loaded {loadedPlugins.Count} plugin(s).");
    }
}
```

먼저 plugins 폴더 내의 모든 DLL 파일의 목록을 가져온다. 에러같은 건 신경쓰지 않는다.

```csharp
var dllFiles = Directory.GetFiles(
    Path.Combine(Directory.GetCurrentDirectory(), "plugins"),
    "*.dll",
    SearchOption.AllDirectories
);
```

`dllFiles`는 `string?[]` 타입으로 plugins 폴더 안의 모든 DLL 파일을 가리키는 경로를 갖고 있게 된다.

이제 각 DLL 파일을 불러와서, 안에서 `IPlugin` 타입의 클래스를 찾아 `loadedPlugins`에 집어넣는다. 에러같은 건 신경쓰지 않는다.

```csharp
foreach (var dll in dllFiles) {
    var assembly = Assembly.LoadFile(dll);
    var pluginsInAssembly = from type in assembly.GetTypes()
                            where type.IsClass &&
                                  type.GetInterfaces().Select(i => i is IPlugin).Any() &&
                                  !type.IsDefined(typeof(CompilerGeneratedAttribute))
                            select (IPlugin?)Activator.CreateInstance(type);
    foreach (var plugin in pluginsInAssembly) if (plugin != null) loadedPlugins.Add(plugin);
}
```

`Assembly.LoadFile(string path)`는 `path`의 DLL 파일을 불러온 뒤 해당 파일의 .NET 어셈블리를 리턴한다.

그 다음의 LINQ 문은 불러온 어셈블리에서 `IPlugin` 타입의 클래스를 찾아내 그 인스턴스를 만들어 반환한다. 한 줄씩 뜯어보면 다음과 같다.

```csharp
// 불러온 어셈블리에 들어있는 모든 타입 중에서
from type in assembly.GetTypes()
      // 클래스이고
where type.IsClass &&
      // 상속받은 인터페이스로 IPlugin을 가지고
      type.GetInterfaces().Select(i => i is IPlugin).Any() &&
      // 컴파일러가 생성한 것이 아닌 타입을 모두 선택하여
      !type.IsDefined(typeof(CompilerGeneratedAttribute))
// 선택한 타입의 인스턴스를 만든 것의 목록을 리턴한다
select (IPlugin?)Activator.CreateInstance(type);
```

== 이 코드의 추가 설명 ==

```csharp
type.GetInterfaces().Select(i => i is IPlugin).Any()
```

`IEnumerable.Select(Func<Type, bool> selector)`는 `IEnumerable`이 가지는 요소 중 변수로 주어진 함수가 `true`를 반환하는 요소만을 요소로 가지는 `IEnumerable`을 리턴한다.

`IEnumerable.Any()`는 해당 `IEnumerable`에 요소가 한 개라도 있는지를 `bool`로 리턴한다.

따라서 해당 줄 전체는 `type.GetInterfaces()`에 `i => i is IPlugin`을 만족하는 요소가 있는지를 나타내는 `bool` 값이 된다.

```csharp
!type.IsDefined(typeof(CompilerGeneratedAttribute))
```

컴파일러가 생성한 클래스를 걸러내는 이유는 우리의 플러그인 클래스에 존재하는 `async` 함수 때문이다.

`async` 함수는 내부적으로 `await` 분기에 따라 여러 개의 함수로 쪼개지며, 이 쪼개진 함수가 실행이 재개될 때 이전의 실행 컨텍스트를 유지할 수 있게 하기 위해 컴파일러는 먼저 실행 컨텍스트를 들고 있을 수 있는 클래스를 지어낸 뒤, 쪼개진 함수를 해당 클래스 안에 집어넣어놓는다. 이 클래스는 우리가 찾는 클래스와 같은 타입을 가지지만, 컴파일러가 `async` 구현을 위해 자동으로 만든 클래스를 우리가 직접 호출할 이유가 전혀 없기 때문에, 직접 호출할 클래스를 찾고 있는 지금은 걸러내야 한다.

만약 여기서 이런 클래스들을 걸러내지 않으면, 다음과 비슷한 모양의 클래스 목록이 반환될 것이다.

```
CatPlugin
DogPlugin
<GoToSleepAsync>d__14
<GoToSleepAsync>d__15
```

여기서 `CompilerGeneratedAttribute`가 선언되어 있는 클래스를 걸러내면 아래 2개의 이상한 클래스들을 걸러낼 수 있다.

```csharp
select (IPlugin?)Activator.CreateInstance(type);
```

`Activator.CreateInstance(Type type)`는 `type` 타입의 새로운 인스턴스를 생성하여 `object?` 타입으로 리턴한다. 대충 이런 걸 하는 셈이다.

```csharp
object? Activator::CreateInstance(Type type) {
    try {
        return new typeof(type)() as object;
    } catch (Exception) {
        return null;
    }
}
```
<div class="img-cap">실행되는 코드는 아니다, 걍 이런 느낌</div>

== 추가 설명 끝 ==

아무튼 이렇게 공급자 어셈블리 안의 `IPlugin` 상속 클래스의 인스턴스를 하나씩 만들어서 `loadedPlugins` 안에 모두 긁어모으는 데 성공했다!

사용은 그냥 `IPlugin` 사용하듯 사용하면 된다.

```csharp
foreach (var plugin in loadedPlugins) {
    Console.WriteLine($"Using plugin {plugin.Name}");
    plugin.MakeSound();
    await plugin.GoToSleepAsync();
}
```

완성된 플러그인 소비자의 전체 코드는 다음과 같다.

```csharp
using System.Reflection;
using System.Runtime.CompilerServices;

public class Program {
    static async Task Main(string[] args) {
        Console.WriteLine("Loading plugins...");

        List<IPlugin> loadedPlugins = new();

        var dllFiles = Directory.GetFiles(
            Path.Combine(Directory.GetCurrentDirectory(), "plugins"),
            "*.dll",
            SearchOption.AllDirectories
        );

        foreach (var dll in dllFiles) {
            var assembly = Assembly.LoadFile(dll);
            var pluginsInAssembly = from type in assembly.GetTypes()
                                    where type.IsClass &&
                                          type.GetInterfaces().Select(i => i is IPlugin).Any()
                                          !type.IsDefined(typeof(CompilerGeneratedAttribute))
                                    select (IPlugin?)Activator.CreateInstance(type);
            foreach (var plugin in pluginsInAssembly) if (plugin != null) loadedPlugins.Add(plugin);
        }

        Console.WriteLine($"Loaded {loadedPlugins.Count} plugin(s).");

        // plugins are loaded
        foreach (var plugin in loadedPlugins) {
            Console.WriteLine($"Using plugin {plugin.Name}");
            plugin.MakeSound();
            await plugin.GoToSleepAsync();
        }
    }
}
```

이 코드를 실행해보면 정상 작동하는 것을 확인할 수 있다.

```
C:\...\Consumer>dotnet run
Loading plugins...
Loaded 2 plugin(s).
Using plugin Cat
meow
The cat has fallen asleep
Using plugin Dog
woof woof
The dog has fallen asleep

C:\...\Consumer>
```

## 소스 코드

이 샘플 프로젝트의 소스 코드는 내 GitHub에 올려두었다.

적어도 미래의 나에게는 도움이 되길 바라며...

[레포 구경가기](https://github.com/sinusinu/DotnetPluginSystemSample)