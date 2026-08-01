---
title: "C#으로 단순한 플러그인 시스템 구축하기"
date: 2024-03-20 22:16:00 +0900
last_modified_at: 2026-08-01 16:00:00 +0900
categories: [ "컴푸터" ]
excerpt: "정말 단순한 예제 포함"
---

## 아! 메챠쿠챠

C#으로 플러그인 시스템을 구축하려고 인터넷에서 관련 자료를 찾아봤더니, 너무 복잡하고 어려운 글만 잔뜩 나와서 내용을 이해하는 데 좀 고생을 했다.

나중에 필요할 때 다시 보고 빠르게 이해할 수 있도록, 내가 대충 이해한 플러그인 시스템 구축법을 요약해서 여기 정리해둔다.

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
<div class="code-cap">이렇게 하면 안 된다</div>

결국 인터페이스가 한 어셈블리에서만 와야 하는데 공급자에게서는 당연히 올 수 없고, 공급자에게 소비자를 레퍼런스로 추가하게 하는 건 배꼽이 배보다 더 커지는 꼴이 되니, 이래저래 생각해본 결과 다음과 같은 방식이 가장 적절하다고 판단했다.

1. 인터페이스만 제공하는 베이스 프로젝트를 별도로 생성한다.
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
<div class="code-cap">되긴 된다</div>

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

이제 공급자 프로젝트를 빌드해 플러그인 2개가 담긴 DLL 파일을 뽑아낸 뒤, 소비자 프로젝트의 작업 디렉토리에 `plugins` 폴더를 만들고 그 안에 집어넣자. 인터페이스만 들어있는 베이스 DLL 파일은 어차피 소비자가 베이스 프로젝트를 참조하고 있으므로 굳이 갖다둘 필요는 없다.

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

어셈블리가 불러와지면 이제 LINQ 문을 사용해 조건에 맞는 클래스의 인스턴스들을 만들어 가지고 올 수 있다.

```csharp
// 불러온 어셈블리에 들어있는 모든 타입 중에서
from type in assembly.GetTypes()
// 클래스이고
where type.IsClass &&
      // IPlugin 인터페이스를 상속했고
      type.GetInterfaces().Select(i => i is IPlugin).Any() &&
      // 컴파일러가 생성한 타입(async 등)이 아닌 타입에 대해
      !type.IsDefined(typeof(CompilerGeneratedAttribute))
// 선택한 타입의 인스턴스를 만든 것의 목록을 리턴한다
select (IPlugin?)Activator.CreateInstance(type);
```

이렇게 하면 공급자 어셈블리 안의 `IPlugin` 상속 클래스의 인스턴스를 하나씩 만들어서 `loadedPlugins` 안에 모두 긁어모을 수 있다.

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