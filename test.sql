-- 建立資料表
CREATE TABLE Device (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Type NVARCHAR(50)
);

CREATE TABLE TestResult (
    Id INT PRIMARY KEY IDENTITY(1,1),
    DeviceId INT NOT NULL,
    TestType NVARCHAR(50), 
    Value FLOAT,
    Unit NVARCHAR(20),
    TestDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (DeviceId) REFERENCES Device(Id)
);

-- 塞入一筆初始資料
INSERT INTO Device (Name, Type) VALUES ('SSD-NVMe-Gen4', 'Storage');
INSERT INTO TestResult (DeviceId, TestType, Value, Unit) VALUES (1, 'IOPS', 7500.5, 'k');