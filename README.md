# AvalonMobile

Projeto base criado a partir do template **blank** do **.NET MAUI**.

## ✅ Status atual verificado

O ambiente foi validado com sucesso:

- `dotnet build .\Avalon.csproj -f net10.0-android` → **build OK**
- `emulator -list-avds` → AVD criado: **`Avalon_API_36`**
- `adb devices` → emulador online: **`emulator-5554`**
- `adb shell pm list packages` → app instalado: **`com.companyname.avalon`**

---

## 🚀 Rodar com 1 comando

Use o script abaixo na raiz do projeto:

```powershell
.\run-android.ps1
```

Se quiser informar outro emulador:

```powershell
.\run-android.ps1 -AvdName "NomeDoSeuAVD"
```

---

## 📋 Passo a passo manual para rodar

### 1) Abrir o emulador

```powershell
$env:ANDROID_SDK_ROOT="$env:LOCALAPPDATA\Android\Sdk"
$env:PATH="$env:ANDROID_SDK_ROOT\emulator;$env:ANDROID_SDK_ROOT\platform-tools;$env:PATH"
emulator -avd Avalon_API_36
```

### 2) Configurar Java/JDK na sessão

```powershell
$env:JAVA_HOME="C:\Program Files\Android\Android Studio\jbr"
$env:ANDROID_SDK_ROOT="$env:LOCALAPPDATA\Android\Sdk"
$env:PATH="$env:JAVA_HOME\bin;$env:ANDROID_SDK_ROOT\platform-tools;$env:ANDROID_SDK_ROOT\emulator;$env:PATH"
```

### 3) Rodar o app MAUI no Android

```powershell
dotnet build .\Avalon.csproj -t:Run -f net10.0-android `
  -p:JavaSdkDirectory="$env:JAVA_HOME" `
  -p:AndroidSdkDirectory="$env:ANDROID_SDK_ROOT"
```

---

## 🔁 Comandos úteis do dia a dia

### Ver dispositivos conectados
```powershell
adb devices
```

### Listar emuladores disponíveis
```powershell
emulator -list-avds
```

### Compilar para Android
```powershell
dotnet build .\Avalon.csproj -f net10.0-android
```

### Compilar e abrir no emulador
```powershell
dotnet build .\Avalon.csproj -t:Run -f net10.0-android
```

### Ver logs do app/dispositivo
```powershell
adb logcat
```

### Fechar o app no emulador
```powershell
adb shell am force-stop com.companyname.avalon
```

---

## 🧱 Estrutura do projeto

Este template está bem enxuto. Os arquivos principais são:

| Arquivo / Pasta | Função |
|---|---|
| `Avalon.csproj` | Configuração do projeto, targets (`android`, `ios`, `windows`), pacotes e metadados do app |
| `MauiProgram.cs` | Ponto de inicialização do MAUI; lugar ideal para registrar serviços, `HttpClient`, DI e logging |
| `App.xaml` | Recursos globais da aplicação, como estilos e cores |
| `App.xaml.cs` | Criação da janela principal; hoje ele sobe o `AppShell` |
| `AppShell.xaml` | Estrutura de navegação do app (`Shell`), rotas e página inicial |
| `MainPage.xaml` | Interface visual da tela inicial |
| `MainPage.xaml.cs` | Lógica da tela inicial; no template, faz o contador do botão |
| `Resources/Styles/` | Cores e estilos globais reutilizáveis |
| `Resources/Images/`, `Fonts/`, `Splash/`, `AppIcon/` | Assets visuais do app |
| `Platforms/Android/`, `iOS/`, `Windows/` | Código específico de cada plataforma |

---

## 🧭 Como começar a desenvolver

Sugestão de fluxo inicial:

1. **Mexa em `MainPage.xaml`** para aprender a montar layout.
2. **Use `MainPage.xaml.cs`** para testar eventos e comportamento simples.
3. **Crie novas telas** em uma pasta como `Pages/`.
4. **Adicione navegação** no `AppShell.xaml`.
5. **Registre serviços** no `MauiProgram.cs` quando começar a estruturar melhor o app.

### Exemplo de começo prático

- trocar o texto `Hello, World!`
- mudar cores em `Resources/Styles/Colors.xaml`
- criar uma segunda página, como `LoginPage.xaml`
- navegar para ela via `Shell`

---

## 💡 Dica importante

Para evitar configurar tudo a cada sessão, vale definir no Windows estas variáveis de ambiente:

- `JAVA_HOME = C:\Program Files\Android\Android Studio\jbr`
- `ANDROID_SDK_ROOT = C:\Users\jpdat\AppData\Local\Android\Sdk`

---

## Próximo passo sugerido

Quando quiser, o próximo passo natural é eu te ajudar a montar uma estrutura mais organizada com pastas como:

- `Pages/`
- `ViewModels/`
- `Services/`
- `Models/`

Isso já deixa o projeto pronto para começar o desenvolvimento de verdade.
