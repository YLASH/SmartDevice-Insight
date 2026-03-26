// Console.WriteLine("Hello, World!");
using Microsoft.Data.SqlClient;
string connectionString = "Server=.\\SQLEXPRESS;Database=master;Integrated Security=True;TrustServerCertificate=True;";

try 
{
    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        // Console.WriteLine("📡 正在撥通 SQL Server (SQLEXPRESS)...");
        connection.Open();
        Console.WriteLine("✅ 已連線至系統。");
        
        bool exitSystem = false;
        while (!exitSystem) // main loop for the whole system, will keep running until user chooses to exit
        {
            Console.WriteLine("\n======= 🛠️ 設備測試管理系統 =======");
            Console.WriteLine("[1] 開始新增測試紀錄");
            Console.WriteLine("[2] 直接查看目前報告");
            Console.WriteLine("[3] 結束並離開");
            Console.Write("請選擇操作: ");
            string? mainMenuChoice = Console.ReadLine();

            if (mainMenuChoice == "1")
            {
                string? userAdding;
                int totalAdded = 0; // add a counter to track how many records we added in this session
                do 
                {
                    // --- 新增資料階段 ---
                    Console.WriteLine("\n--- 📝 輸入新測試資料 ---");
                    //Furture adjust: we can add a device selection menu here, and let user choose which device they are adding data for. 
                    // For now we will just hardcode DeviceId = 1 (SSD) in the SQL INSERT statement below.
                    // Console.Write("請輸入設備 ID (1: SSD, 2: HDD): ");
                    // int selectedDeviceId = int.Parse(Console.ReadLine() ?? "1");

                    Console.Write("請輸入測試項目 (例如: Speed, Temp):")  ;
                    string? testType = Console.ReadLine();

    
                    // --- inter value ---
                    double value; 
                    while (true) 
                    {
                        Console.Write("請輸入數值 (例如: 500.5): ");
                        string? input = Console.ReadLine();

                        // TryParse transforms the input string into a double. 
                        if (double.TryParse(input, out value)) 
                        {
                            break; // Success while loop, continue to execute SQL
                        }
                        else 
                        {
                            // Conversion failed, display warning, and since while(true) will ask again
                            Console.WriteLine("⚠️  輸入錯誤！請輸入『純數字』，不要輸入文字或符號。");
                        }
                    }                   

                    Console.Write("請輸入單位 (例如: Mbps, °C): ");
                    string? unit = Console.ReadLine();


                    // SQL INSERT
                    string insertSql = @"INSERT INTO TestResult (DeviceId, TestType, Value, Unit) 
                                VALUES (1, @TestType, @Value, @Unit)";

                    using (SqlCommand insertCommand = new SqlCommand(insertSql, connection))
                    {
                        // For now we will just hardcode DeviceId = 1 (SSD) in the SQL INSERT statement above, but in the future we can replace it with a parameter and let user choose which device they are adding data for.
                        // insertCommand.Parameters.AddWithValue("@deviceId", selectedDeviceId); 
                        insertCommand.Parameters.AddWithValue("@TestType", testType ?? "Unknown");
                        insertCommand.Parameters.AddWithValue("@Value", value);
                        insertCommand.Parameters.AddWithValue("@Unit", unit ?? "");
                        insertCommand.ExecuteNonQuery();
                    }

                    totalAdded++; // 每成功一筆就加 1
                    Console.WriteLine($"✅ 已暫存第 {totalAdded} 筆數據...");

                    // check user want to continue or not, only accept Y/N, otherwise keep asking
                    while (true) {
                        Console.Write("是否繼續輸入下一筆資料？(Y/N): ");
                        userAdding = Console.ReadLine()?.ToUpper();
                        if (userAdding == "Y" || userAdding == "N") break;
                        Console.WriteLine("⚠️ 請輸入 Y 或 N 喔！");
                    }

                } while (userAdding == "Y");

                Console.WriteLine("\n🚀 新增完成！正在彙總最新報表...");
                ShowReport(connection);
            }
            else if (mainMenuChoice == "2")
            {
                ShowReport(connection);

                Console.WriteLine("\n報表讀取完畢，按 Enter 回到主選單...");
                Console.ReadLine();
            }
            else if (mainMenuChoice == "3")
            {
                exitSystem = true; 
                Console.WriteLine("👋 感謝使用，再見！");
            }
            else
            {
                Console.WriteLine("❌ 無效選擇，請重新輸入。");
            }

        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ 發生錯誤: {ex.Message}");
}

// last breaking line before end of program, to make the output look nicer and give user a chance to see the final message before closing the console window.
Console.WriteLine("\n------------------------------------");
Console.WriteLine("系統已結束，請按 Enter 鍵關閉視窗...");
Console.ReadLine();



//【Method keep in bottom for better readability 】
// ---------------------------------------------------------
// ShowReport Method: will query the database for all test results, and display them in a formatted table with status analysis. We will reuse the same connection object that was opened in the main method, so we don't need to open it again here. Just execute the command directly.
void ShowReport(SqlConnection conn)
{
    {
        // ShowReport
        // 【Head line】header and Title
        Console.WriteLine("\n" + new string('=', 75)); // 印出 75 個 =
        Console.WriteLine($"║ {"[ 裝置名稱 ]",-20} │ {"[ 項目 ]",-10} │ {"[ 數值 ]",-10} │ {"[ 狀態 ]",-15} ");
        Console.WriteLine(new string('-', 72)); // 印出分隔線
        
        string selectSql = "SELECT D.Name, T.TestType, T.Value, T.Unit FROM Device D JOIN TestResult T ON D.Id = T.DeviceId";
        
        // notice: we are reusing the same connection object 'conn' that was passed in, so we don't need to open it again here. Just execute the command directly.
        using (SqlCommand selectCommand = new SqlCommand(selectSql, conn))
        using (SqlDataReader reader = selectCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                string name = reader["Name"]?.ToString() ?? "N/A";
                string type = reader["TestType"]?.ToString() ?? "N/A";
                double val = Convert.ToDouble(reader["Value"]); // 轉成數字才能比大小
                string unt = reader["Unit"]?.ToString() ?? "";


               //Status anaysis =>Furture adjuest: Set up more conditions for different test types and values, and assign a status string accordingly.
               string status = GetStatus(name, type, val);
               
                // 2. formatted output with alignment and status
                // {val,8} means: print val in a field of width 8, aligned to the right. 
                // This way all numbers will line up nicely on the right side, even if they have different lengths.
                Console.WriteLine($"║ {name,-20} │ {type,-12} │ {val,6} {unt,-6} │ {status,-15} ");
                
            }
            Console.WriteLine(new string('=', 70));
        }
        // Console.WriteLine("\n報表讀取完畢，按 Enter 回到主選單...");
        // Console.ReadLine();
    }
}

// Status analysis method, will return a status string based on the device name, test type and value. We can set up different conditions for different devices and test types, and assign a status string accordingly.
string GetStatus(string deviceName, string testType, double val)
{
    if (deviceName.Contains("SSD"))
    {
        if (testType == "Temp" && val > 70) return "🔥 [SSD過熱]";
        if (testType == "Speed" && val < 500) return "🐌 [寫入過慢]";
    }
    else if (deviceName.Contains("Network"))
    {
        if (testType == "Speed" && val < 10) return "❌ [網路斷線]";
    }
    
    return "✅ [正常]"; // 預設狀態
}