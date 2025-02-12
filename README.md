# UsernameCheckerWPF 🚀

### **A WPF application that detects inappropriate usernames using regex, AI moderation, and OpenAI GPT-based filtering.**

## **📌 Features**
✅ **Upload Files**: Supports **Excel (.xlsx)** and **Text (.txt)** files for bulk username checking.  
✅ **AI-Powered Filtering**: Uses **OpenAI’s Moderation API** and **GPT-3.5-Turbo** to detect offensive usernames.  
✅ **Manual Filtering**: Uses **regex-based detection** to catch obfuscated profanity.  
✅ **Leetspeak Decoding**: Converts leetspeak before AI analysis.  
✅ **Multi-threaded Processing**: Avoids UI freezing with background tasks.  
✅ **Export Results**: Save inappropriate usernames to **TXT or Excel files**.  

---

## **🔧 Setup Instructions**
### **1️⃣ Install Dependencies**
Ensure you have the required **NuGet packages** installed in Visual Studio:
```sh
Install-Package Newtonsoft.Json
Install-Package ClosedXML
Install-Package Microsoft.Win32.Registry
