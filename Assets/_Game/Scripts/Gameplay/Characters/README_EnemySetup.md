# Enemy Character Setup Guide

Bu rehber, Unity'de hasar alabilen düşman karakterlerin nasıl kurulacağını açıklar.

## Gerekli Komponentler

### 1. EnemyCharacter.cs
- Ana düşman karakteri scripti
- `IHittable` interface'ini implement eder
- Sağlık sistemi ve hasar alma mantığını yönetir

### 2. EnemyAIStateMachine.cs (İsteğe bağlı)
- Düşman AI davranışlarını yönetir
- Artık `IHittable` interface'ini de implement eder
- `EnemyCharacter` ile otomatik entegrasyon

### 3. EnemyHealthBar.cs (İsteğe bağlı)
- Düşman sağlık çubuğu UI komponenti
- Hasar aldıkında görünür hale gelir
- Kamera yönünde döner

## Prefab Kurulumu

### Adım 1: Basic GameObject Setup
```
Enemy GameObject
├── Model (3D model veya sprite)
├── Collider (BoxCollider, CapsuleCollider, vb.)
├── Rigidbody (Physics için)
├── EnemyCharacter script
├── EnemyAIStateMachine script (isteğe bağlı)
└── HealthBar Canvas (UI için)
```

### Adım 2: EnemyCharacter Ayarları
```csharp
// Inspector'da ayarlanması gerekenler:
- Max Health: 100 (düşman sağlığı)
- Destroy On Death: true (ölümde yok et)
- Death Delay: 2f (ölüm animasyonu için bekleme)
- Damage Effect: Hasar efekti prefabı
- Death Effect: Ölüm efekti prefabı
- Audio Source: Ses efektleri için
- Damage Sound: Hasar ses efekti
- Death Sound: Ölüm ses efekti
```

### Adım 3: Health Bar Setup (İsteğe bağlı)
```
HealthBar Canvas (World Space)
├── Background Image
└── Health Slider
    └── Fill Area
        └── Fill (Health bar dolgusu)
```

### Adım 4: Animator Setup (İsteğe bağlı)
Animator Controller'da şu trigger'lar olmalı:
- `TakeDamage`: Hasar alma animasyonu
- `Die`: Ölüm animasyonu

## Kullanım

### Basit Düşman
```csharp
// Sadece EnemyCharacter scripti ekleyin
// Projectile'lar otomatik olarak hasar verecektir
```

### AI'lı Düşman
```csharp
// Hem EnemyCharacter hem EnemyAIStateMachine ekleyin
// AI otomatik olarak ölüm durumunu handle edecektir
```

### Sağlık Çubuklu Düşman
```csharp
// EnemyCharacter + EnemyHealthBar scripti ekleyin
// Health bar otomatik olarak sağlık durumunu gösterecektir
```

## Kod Örnekleri

### Manuel Hasar Verme
```csharp
EnemyCharacter enemy = GetComponent<EnemyCharacter>();
enemy.TakeDamage(25f);
```

### Ölüm Eventini Dinleme
```csharp
EnemyCharacter enemy = GetComponent<EnemyCharacter>();
enemy.OnEnemyDeath += () => {
    Debug.Log("Enemy died!");
    // Ödül verme, skorlama, vb.
};
```

### Hasar Eventini Dinleme
```csharp
EnemyCharacter enemy = GetComponent<EnemyCharacter>();
enemy.OnDamageReceived += (damage) => {
    Debug.Log($"Enemy took {damage} damage!");
    // UI güncelleme, efektler, vb.
};
```

## Projectile Entegrasyonu

Projectile'lar zaten `IHittable` interface'ini kullanarak düşmanlara hasar veriyor. `EnemyCharacter` scripti bu interface'i implement ettiği için otomatik olarak çalışacaktır.

### ProjectileSystem ile Uyumluluk
- `ProjectileSystem.cs` zaten `IBaseDamageProvider` kullanarak hasar veriyor
- `EnemyCharacter.OnReceivedHit()` metodu bu hasarı alıp `HealthSystem`'e iletiyor
- Knockback, efektler ve diğer hit response'lar otomatik olarak çalışıyor

## Tips

1. **Performans**: Çok sayıda düşman varsa, health bar'ları sadece hasar aldıklarında gösterin
2. **Efektler**: Hasar ve ölüm efektlerini prefab olarak hazırlayın
3. **Sesler**: AudioSource'u düşman prefabında tutun, sesler spatial olsun
4. **Animasyon**: Trigger-based animasyonlar kullanın, smooth geçişler için
5. **Debugging**: Console loglarını aktif/pasif etmek için `showHealthInConsole` kullanın

## Sorun Giderme

### Düşman Hasar Almıyor
- Collider'ın `IsTrigger = false` olduğundan emin olun
- `EnemyCharacter` scriptinin enabled olduğunu kontrol edin
- Projectile'ın doğru layer'da olduğunu kontrol edin

### Health Bar Görünmüyor
- Canvas'ın `World Space` modunda olduğundan emin olun
- Camera referansının doğru olduğunu kontrol edin
- UI elementlerinin aktif olduğunu kontrol edin

### AI Çalışmıyor
- `EnemyAIStateMachine` ile `EnemyCharacter` aynı GameObject'te olmalı
- Core target'ının atandığından emin olun
- Player tag'lerinin doğru olduğunu kontrol edin