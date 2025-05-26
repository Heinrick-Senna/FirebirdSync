# FirebirdSync

Aplicativo Windows Forms para sincronização de bancos de dados Firebird entre dois servidores, utilizando `gbak`.

## Funcionalidades

- Backup e restore entre dois bancos Firebird remotos ou locais.
- Na primeira execução, o aplicativo irá solicitar o caminho do `gbak.exe`.
- Armazenamento de configurações de IPs, caminhos e credenciais em `%APPDATA%/firebirdsync`.
- Interface gráfica simples com dois botões: **Enviar** (origem → destino) e **Baixar** (destino → origem).
- Geração de logs de saída e erro (`last_output.log` e `last_error.log`).
- Build em `.exe` único com ícone customizado.

---

## Requisitos

- .NET 6.0 SDK+
- Docker (opcional, para simular servidores Firebird)
- `gbak.exe` do Firebird disponível

---

## Estrutura de Pastas

```
%APPDATA%/firebirdsync/
├── configGBAK.txt        # Caminho do gbak.exe
├── configIPS.txt         # IPs, caminhos e credenciais
├── backup.fbk            # Backup temporário
├── last_output.log       # Saída do comando
└── last_error.log        # Erros do comando
```

---

## Configuração do Docker Para Testes (opcional)

Crie um arquivo `docker-compose.yml`:

```yaml
version: "3.9"
services:
  firebird_origem:
    image: jacobalberty/firebird:3.0
    container_name: firebird_origem
    ports:
      - "3050:3050"
    volumes:
      - ./data/VERSAOATUALIZADA.FDB:/firebird/data/VERSAOATUALIZADA.FDB
    environment:
      ISC_PASSWORD: masterkey

  firebird_destino:
    image: jacobalberty/firebird:3.0
    container_name: firebird_destino
    ports:
      - "3051:3050"
    volumes:
      - ./data/VERSAODESATUALIZADA.FDB:/firebird/data/VERSAODESATUALIZADA.FDB
    environment:
      ISC_PASSWORD: masterkey
```

Execute:

```bash
docker-compose up -d
```

---

## Como rodar em desenvolvimento

Abra o projeto no Visual Studio ou com `dotnet run`:

---

## Para Build

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true
```

O resultado será um `.exe` único em `bin\Release\net6.0-windows\win-x64\publish\`.

---

## Observações

- O ícone da janela principal é configurado dentro do formulário via `this.Icon = new Icon("icon.ico");`.
- Se estiver sem Visual Studio e sem Solution Explorer, edite os arquivos manualmente.

---

## Exemplo de `configIPS.txt`

```txt
ipOrigem=localhost/3050
caminhoOrigem=/firebird/data/VERSAOATUALIZADA.FDB
usuarioOrigem=SYSDBA
senhaOrigem=masterkey
ipDestino=localhost/3051
caminhoDestino=/firebird/data/VERSAODESATUALIZADA.FDB
usuarioDestino=SYSDBA
senhaDestino=masterkey
```

---

## Autor original

Desenvolvido por Marcelo (FullStack dev) com foco em facilidade de manutenção e uso local ou remoto.
