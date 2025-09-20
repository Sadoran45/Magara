# Type-Safe Animation Event System

Bu sistem Unity'de string tabanlı animation event'ler yerine ScriptableObject tabanlı type-safe event sistemi sağlar.

## Bileşenler

### 1. AnimEventSO
- ScriptableObject tabanlı animation event sınıfı
- Inspector'da Create > Animation > Animation Event ile oluşturulabilir
- String, float, int, bool parametreleri içerebilir
- Her event için açıklama ve isim alanları

### 2. IAnimationEventListener
- Event dinleyicileri için interface
- `OnAnimationEvent(AnimEventSO animEvent)` metodunu implement etmelidir

### 3. AnimationEventDispatcher
- Animation event'leri alıp ilgili listener'lara ileten component
- Animator'a sahip GameObject'e eklenmelidir
- `TriggerEvent(AnimEventSO animEvent)` metodu animation event'lerden çağrılır

### 4. FootstepHandler (Örnek)
- IAnimationEventListener implementasyonu
- Footstep event'leri için ses ve partikül efektleri
- Hangi event'leri dinleyeceğini inspector'dan seçebilir

## Kurulum ve Kullanım

### 1. AnimEventSO Asset'i Oluşturma
```
1. Project window'da sağ tık
2. Create > Animation > Animation Event seç
3. Event'e isim ver ve parametreleri ayarla
4. Asset'i kaydet
```

### 2. AnimationEventDispatcher Kurulumu
```
1. Animator component'ine sahip GameObject'i seç
2. AnimationEventDispatcher component'ini ekle
3. Debug logs'u isteğe bağlı olarak aktifleştir
```

### 3. Animation Event Ekleme
```
1. Animation window'da animation clip'i aç
2. Timeline'da event eklemek istediğin frame'i seç
3. Event ekle ve Function olarak "TriggerEvent" seç
4. Object Reference parametresi olarak AnimEventSO asset'ini sürükle
```

### 4. Event Listener Oluşturma
```csharp
public class MyEventHandler : MonoBehaviour, IAnimationEventListener
{
    [SerializeField] private AnimEventSO[] eventsToListenFor;
    private AnimationEventDispatcher dispatcher;

    private void Start()
    {
        dispatcher = GetComponentInParent<AnimationEventDispatcher>();
        
        foreach(var eventSO in eventsToListenFor)
        {
            dispatcher.RegisterListener(eventSO, this);
        }
    }

    public void OnAnimationEvent(AnimEventSO animEvent)
    {
        Debug.Log($"Received event: {animEvent.eventName}");
        // Event'e özel mantığınızı buraya yazın
    }

    private void OnDestroy()
    {
        if(dispatcher != null)
            dispatcher.UnregisterListenerFromAll(this);
    }
}
```

## Avantajlar

1. **Type Safety**: String yerine ScriptableObject referansı kullanılır
2. **Inspector Integration**: Event'ler inspector'da drag-drop ile atanabilir
3. **Parameterization**: Her event farklı parametreler taşıyabilir
4. **Reusability**: Aynı event birden fazla yerde kullanılabilir
5. **Debug Support**: Event triggering'i debug log'larla takip edilebilir
6. **Performance**: Dictionary tabanlı event routing sistemi

## Örnek Kullanım Senaryoları

- **Footstep System**: Yürüme animasyonlarında ses ve partikül efektleri
- **Combat System**: Saldırı animasyonlarında hasar timing'i
- **UI Animations**: Buton animasyonlarında ses efektleri
- **Environmental**: Kapı açma, sandık açma gibi etkileşimler

## Dosya Yapısı
```
Assets/Scripts/AnimationEvents/
├── AnimEventSO.cs                    # ScriptableObject event sınıfı
├── IAnimationEventListener.cs        # Event listener interface
├── AnimationEventDispatcher.cs       # Ana dispatcher component
└── Examples/
    ├── FootstepHandler.cs            # Örnek footstep handler
    └── ExampleAnimEventCreator.cs    # Örnek event asset'leri
```

## İpuçları

1. Event asset'lerinizi organize etmek için klasör yapısı kullanın
2. Event isimlerini açıklayıcı yapın (örn: "LeftFootStep", "SwordHit")
3. Parametre alanlarını tutarlı şekilde kullanın (örn: floatParameter her zaman volume için)
4. Debug log'ları development sırasında açık tutun
5. Event'leri test etmek için inspector'daki context menu'leri kullanın