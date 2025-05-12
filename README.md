# ACID Prensipleri

**Atomicity (Bölünemezlik)**  
Transaction içindeki tüm adımlar ya tamamı başarılı olur ya da hiçbirisi uygulanmaz.  
_Örn: Para transferinde para çekme başarılı ama yatırma başarısızsa tüm işlem geri alınır._

**Consistency (Tutarlılık)**  
Transaction öncesi ve sonrası sistemin kurallarına göre tutarlı bir durumda kalmalıdır.  
_Örn: Hesap bakiyeleri toplamı işlem öncesi ve sonrası aynı olmalı._

**Isolation (Yalıtım)**  
Aynı anda çalışan transaction’lar birbirini etkilemeden çalışmalıdır.  
_Örn: Bir işlem bitmeden başka bir işlem onun ara verilerini görmemeli._

**Durability (Kalıcılık)**  
Transaction başarılı bir şekilde tamamlandıktan sonra, sistem çökse bile değişiklikler kalıcı olur.  
_Veriler genelde diske yazılarak sağlanır._

---

# Isolation Level

- **Read Uncommitted:** Başka transaction’ların henüz commit edilmemiş verilerini okuyabilir.  
- **Read Committed:** Yalnızca commit edilmiş verileri okuyabilir.  
- **Repeatable Read:** Aynı anda çalışan iki client'tan biri `SaveChanges`’a geldiğinde commit olana kadar diğer transaction kitlenir.  
- **Ekstra Koruma:** `UPDLOCK`, `ROWLOCK` gibi kilitler ile aynı anda ikinci bir transaction bu satırı okuyamaz. Yarış durumu (race condition) engellenir.  
- **RowVersion (timestamp):** Transaction sonunda EF Core satırın versiyonunun değişip değişmediğini kontrol eder. Değiştiyse `DbUpdateConcurrencyException` fırlatır, işlem retry edilir veya iptal edilir.

---

# EF Core

`ACID_EFCore` projesi altında isolation seviyeleri değerlendirilmiştir.

---

# Idempotency

**Idempotency**, bir işlem veya API isteği birden fazla kez yapıldığında aynı sonucu döndürmesidir.

_Örn: Para transferi için her seferinde benzersiz bir işlem kimliği (idempotency key) oluşturulur. Bu kimlik ile gelen istek daha önce işlendiyse tekrar işlenmez, önceki sonuç döndürülür._

**Nasıl uygulanır?**
- İstemci işlem kimliği gönderir.
- Sunucu bu kimliği kontrol eder.
- Daha önce işlenmişse tekrar yapılmaz, önceki sonuç döndürülür.
- Bu kimlik işlemle birlikte veritabanında saklanır.

_Bankacılık sistemlerinde duplicate transaction’ları önler._

---

# Para Transferi API Tasarımı (Yüksek Trafik)

**Temel Hedefler:**
- Veri tutarlılığı  
- Yüksek erişilebilirlik  
- Ölçeklenebilirlik  
- Performans  

## Performans ve Yük Yönetimi

- **Load Balancer:** API Gateway, Nginx vb. ile yük dağıtımı  
- **Read-Write Separation:** Okuma için read replica, yazma için master DB  
- **Queue Tabanlı İşlem Yönetimi:** Kafka, RabbitMQ ile işlemler kuyrukta tutulur  
- **Idempotency:** Duplicate işlemler önlenir  
- **Dağıtık Kilitleme:** Redis `SETNX` ile aynı anda birden fazla işlem engellenir  
- **Strong Consistency:** Transaction’lar dikkatli yönetilir (`BeginTransaction`, `Commit`, `Rollback`)  

## Ekstra Önlemler

- **Retry Mechanism:** Başarısız işlem tekrar edilir  
- **Dead Letter Queue (DLQ):** Sürekli hata veren işlemler ayrı kuyruğa alınır  
- **Audit Log:** Kullanıcı işlemleri izlenebilir olmalı  
- **Monitoring / Alerting:** Prometheus, Grafana, ELK stack ile sistem izlenir  

---

# Eventual Consistency (Nihai Tutarlılık)

Tüm node’lar zamanla aynı veri durumuna gelir.  
_Örn: SMS, push notification, loglama gibi kritik olmayan işlemler için kullanılabilir._

---

# Distributed Transaction (Dağıtık İşlem)

Birden fazla veri kaynağında yapılan işlemlerin tutarlılığını sağlar.

## Yaklaşımlar

### 1. 2-Phase Commit (2PC) – Klasik
- `Prepare` ve `Commit` aşamalarından oluşur.  
- Blocking ve yavaştır.  
- Mikroservis mimarisinde önerilmez.

### 2. Saga Pattern – Modern
- Her servis kendi local transaction’ını yapar.  
- Event’ler ile servisler birbirini bilgilendirir.  
- Hata olursa önceki işlemler "compensating transaction" ile geri alınır.

### 3. Outbox Pattern
- DB’ye veri yazılırken aynı transaction içinde bir "outbox" tablosuna event kaydedilir.  
- Event ayrı bir servis tarafından publish edilir.  
- Veri ve event tutarlılığı sağlanır.

---

# Özet

- Mikroservis mimarisi tercih edilmeli  
- Queue tabanlı asenkron mimari kullanılmalı  
- Idempotency ve distributed locking gibi veri tutarlılığı sağlayan yapılar uygulanmalı  
- Strong consistency için transaction’lar dikkatli yönetilmeli  
- Ölçeklenebilirlik için replica, cache, load balancer, queue gibi bileşenler eklenmeli
