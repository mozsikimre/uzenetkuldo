# 💬 C# Szerver-Kliens Chat Alkalmazás (Demo)

Ez a projekt egy egyszerű ablakos alkalmazás C# nyelven, amely szerver-kliens kapcsolatot valósít meg TCP protokoll segítségével. A cél az volt, hogy a kliensek képesek legyenek egymással kommunikálni egy központi szerveren keresztül, privát és publikus üzenetek formájában.

> ⚠️ Ez az alkalmazás egyetemi órai demonstrációs célra készült, nem tartalmaz minden biztonsági és funkcionalitási elemet egy éles rendszerhez.

## 🧩 Funkcionalitás

- 🖥️ **Szerver**:
  - Több kliens kezelése egyszerre.
  - Bejövő üzenetek továbbítása a megfelelő klienseknek.
  - Különbségtétel privát és publikus üzenetek között.

- 👤 **Kliens**:
  - Bejelentkezés felhasználónévvel.
  - Publikus üzenet küldése minden csatlakozott kliensnek.
  - Privát üzenet küldése konkrét kliensnek.
  - Üzenetek megjelenítése egy ablakos felületen (WinForms vagy WPF).

## 🛠️ Technológiák

- **Nyelv**: C#
- **Felület**: Windows Forms (vagy WPF)
- **Kommunikáció**: TCP/IP Socket
- **Platform**: .NET Framework / .NET Core

## 🚀 Használat

### 1️⃣ Szerver indítása

1. Nyisd meg a `Szerver` projektet Visual Studio-ban.
2. Fordítsd és futtasd az alkalmazást.
3. A konzolban megjelenik, hogy a szerver figyel egy adott porton.

### 2️⃣ Kliens indítása

1. Nyisd meg a `Kliens` projektet Visual Studio-ban.
2. Indítsd el az alkalmazást és jelentkezz be.
3. Küldhetsz publikus vagy privát üzeneteket más klienseknek.

## 🎨 Képernyőképek

![App-image](https://github.com/user-attachments/assets/03988f0c-ba35-40c4-bd7a-22e0e8eb8318)

## 📌 Megjegyzés

Ez a projekt csak demonstrációs céllal készült. Nem tartalmaz végponttól végpontig tartó titkosítást vagy felhasználói jogosultságkezelést.
