
# **UsernameCheckerWPF** 🚀

## **🔍 AI-Powered Username Moderation Tool**
A **WPF desktop application** that detects inappropriate usernames using:
- **OpenAI's Moderation API** 🧠
- **GPT-3.5 AI Filtering** 🤖
- **Regex-based profanity detection** 🔎
- **Leetspeak decoding** (e.g., `s3x`, `f@ck`) 🛑

This tool is ideal for **gaming platforms, social media, and chat applications** that need to **prevent offensive usernames**.

---

## **📌 Features**
✅ **Bulk Username Checking**: Upload a `.txt` or `.xlsx` file with usernames  
✅ **AI Moderation**: Uses OpenAI’s **GPT-3.5-Turbo** and **Moderation API**  
✅ **Regex Filtering**: Detects obfuscated profanity (`s3x`, `b@dword`)  
✅ **Leetspeak Decoding**: Converts disguised words before AI analysis  
✅ **Multi-threaded Processing**: Runs API calls without freezing the UI  
✅ **Export Results**: Save detected usernames to **TXT or Excel**  

---

## **🚀 Installation & Setup**
### **1️⃣ Install Required Dependencies**
Ensure your **Visual Studio** project has these **NuGet packages** installed:
```sh
Install-Package Newtonsoft.Json
Install-Package ClosedXML
Install-Package Microsoft.Win32.Registry
```

---

### **2️⃣ Set Up Your OpenAI API Key**
This tool **requires an OpenAI API key** to function.

#### **🔹 Steps to Add Your API Key:**
1. **Go to** [OpenAI API Keys](https://platform.openai.com/api-keys) and generate a new key.
2. **Create a `config.json` file** in the **project root directory** and add:
   ```json
   {
       "OpenAiApiKey": "sk-YOUR_OPENAI_API_KEY"
   }
   ```
3. **Ensure your app reads the API key correctly** from `config.json`.

---

### **3️⃣ Running the Application**
1. **Open the project** in **Visual Studio**.
2. Click **Start (`F5`)** to build and run the application.
3. **Select a `.txt` or `.xlsx` file** containing usernames.
4. Click **"Check Usernames"** to analyze the usernames.
5. If inappropriate usernames are detected, **export results** as `.txt` or `.xlsx`.

---

## **📂 File Uploading Guide**
The application supports two file types:
| File Type | Format Example |
|-----------|---------------|
| **Text (.txt)** | One username per line |
| **Excel (.xlsx)** | Usernames should be in the first column |

---

## **⚠️ Troubleshooting**
### **🔹 API Key Not Found**
❌ **Error:** `"API Key is missing! Ensure it's correctly set in config.json."`  
✅ **Fix:**  
- Ensure your `config.json` is in the **project directory**.
- Make sure `config.json` contains the key **inside double quotes**:
  ```json
  { "OpenAiApiKey": "sk-YOUR_VALID_KEY" }
  ```

### **🔹 "Too Many Requests" (Rate Limit)**
❌ **Error:** `"You exceeded your current quota"`  
✅ **Fix:**  
- Check **[OpenAI Usage](https://platform.openai.com/account/usage)** for quota limits.
- Use **GPT-3.5-Turbo** instead of GPT-4 to reduce costs:
  ```csharp
  model = "gpt-3.5-turbo"; // ✅ Faster and cheaper
  ```

### **🔹 "Misused Header Name 'Content-Type'"**
❌ **Error:** `"Misused Header Name 'Content-Type'"`  
✅ **Fix:**  
- Ensure `Content-Type` is **only set inside HttpContent**:
  ```csharp
  HttpContent content = new StringContent(jsonBody, Encoding.UTF8);
  content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
  ```

---

## **📜 License**
This project is licensed under the **MIT License**. Feel free to use and modify!

📌 **Developer:** vlyot  

---
```
