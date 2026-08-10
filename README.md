# 💬 Chat P2P — Sistema di Messaggistica Peer-to-Peer

[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://dotnet.microsoft.com/)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![WinForms](https://img.shields.io/badge/WinForms-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/desktop/winforms/)
[![TCP](https://img.shields.io/badge/TCP%2FIP-000000?style=for-the-badge&logo=internetcomputer&logoColor=white)](https://learn.microsoft.com/dotnet/fundamentals/networking/sockets/)

Sistema di chat **client-server** in C# WinForms con architettura **TCP/IP**, evoluto in tre versioni progressive per esplorare il modello **peer-to-peer**.

---

## ✨ Versioni del Progetto

### 1. `chatserver1.0` — Server Centralizzato
Server TCP classico che accetta connessioni multiple e inoltra i messaggi a tutti i client connessi.

### 2. `chatclientp2p` — Client WinForms
Interfaccia grafica Windows Forms per connettersi al server, inviare e ricevere messaggi in tempo reale.

### 3. `chatserverP2p2.0` — Server P2P Evoluto
Versione avanzata del server con funzionalità peer-to-peer migliorate:
- Routing diretto tra client
- Gestione disconnessioni
- Riconnessione automatica

---

## 🛠️ Stack Tecnologico

| Categoria | Tecnologia |
|-----------|-----------|
| **Linguaggio** | C# (.NET Framework 4.x) |
| **GUI** | Windows Forms |
| **Rete** | TCP/IP Socket (`System.Net.Sockets`) |
| **Threading** | `System.Threading.Tasks`, `async/await` |
| **Gestione Codice** | Qodana (analisi statica) |

---

## 📂 Struttura

```text
Progetti-P2P/
├── chatclientp2p/
│   ├── chatclientp2p/
│   │   ├── Program.cs          # Entry point client
│   │   ├── Form1.cs            # UI chat client
│   │   ├── MainForm.cs         # UI alternativa
│   │   └── App.config          # Configurazione endpoint
│   └── chatclientp2p.sln
├── chatserver1.0/
│   ├── chatserver1.0/
│   │   ├── Program.cs          # Entry point server
│   │   ├── Form1.cs            # UI admin server
│   │   ├── packages.config     # Dipendenze NuGet
│   │   └── App.config
│   └── chatserver1.0.sln
└── chatserverP2p2.0/
    ├── chatserverP2p2.0/
    │   ├── Program.cs          # Entry point server P2P
    │   ├── Form1.cs            # UI admin avanzata
    │   └── App.config
    └── chatserverP2p2.0.sln
```

---

## 🚀 Quick Start

### Prerequisiti

- Visual Studio 2022 (o Visual Studio Code + .NET Framework SDK)
- .NET Framework 4.7.2 o superiore

### 1. Avvia il server

```bash
cd chatserver1.0
msbuild chatserver1.0.sln
```

Oppure apri `chatserver1.0.sln` in Visual Studio e premi F5.

### 2. Avvia uno o più client

```bash
cd chatclientp2p
msbuild chatclientp2p.sln
```

### 3. Testa la comunicazione

- Inserisci l'IP del server nel client (default: `127.0.0.1`)
- Invia un messaggio: verrà inoltrato a tutti i client connessi

---

## 🏗️ Architettura

```
┌──────────┐     TCP      ┌──────────────┐     TCP      ┌──────────┐
│  Client  │◄─────────────►│    Server    │◄─────────────►│  Client  │
│  WinForms│               │   WinForms   │               │  WinForms│
└──────────┘               └──────────────┘               └──────────┘
                                  │
                                  │ P2P v2.0
                                  ▼
                      ┌────────────────────┐
                      │  Routing diretto   │
                      │  Client ↔ Client   │
                      └────────────────────┘
```

---

## 📝 Note

- Progetto didattico per apprendere le basi della comunicazione TCP/IP in C#
- Le tre versioni riflettono l'evoluzione dal modello client-server al peer-to-peer
- Il file `qodana.yaml` contiene la configurazione per l'analisi statica del codice

---
