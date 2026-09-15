# BrokRace

Tek pistte 1 oyuncu + 7 rakiple oynanan 3D yarış denemesi. Araba kendi kendine gidiyor,
direksiyon yok. Oyuncunun tek işi ekrandaki **x1–x5** butonlarıyla doğru anda hızlanmak.

---

## Kurulum ve çalıştırma

- **Unity sürümü:** 6000.3.10f1
- **Render pipeline:** URP 17.3.0 (asset paketi Built-in içindi, materyalleri URP/Lit'e çevirdim)
- **Açılacak sahne:** `Assets/Scenes/Race.unity` — oyunda tek sahne var
- **Build:** `Build/BrokRace.exe` — açınca önce seçim ekranı geliyor, START'a basınca yarış başlıyor
- **Ek bağımlılıklar:** UniTask, Addressables, Input System ve kendi yazdığım PoolManager paketi
  (`com.batuhankanbur.poolmanager`). Hepsi `Packages/manifest.json` içinde, proje açılınca
  kendi kendine iniyor.

Projeyi klonlayıp açtığında Addressables içeriği `Assets/Settings/AddressableAssetsData`
altında hazır geliyor. Yeniden üretmek gerekirse `PlayerBuilder.BuildContent`.

---

## Kontroller ve kurallar

### Butonlar

Klavye yok, her şey ekrandan.

| Buton | Ne yapıyor |
|---|---|
| `x1` – `x5` (altta) | Hızlanma isteği gönderiyor |
| `STATS` (sağ altta) | Detaylı bilgileri açıp kapatıyor |
| `RESTART` (sonuç ekranı) | Aynı ayarlarla yeni yarış |
| `MODES` (sonuç ekranı) | Seçim ekranına dön |

Her dokunuş bir istek sayılıyor. Parmağı basılı tutmanın faydası yok.

### Seçim ekranı

Oyun açılınca ve `MODES` ile buraya geliyorsun. Buradaki seçimler **sadece rakipleri**
etkiliyor, oyuncu her zaman butonlarla oynuyor.

| `RIVAL BOOSTS` | Rakipler ne yapıyor |
|---|---|
| `NORMAL` | Her rakip kendi karakterine göre karar veriyor |
| `NO BOOST` | Rakipler hiç hızlanmıyor |
| `SPAM x5` | Rakipler durmadan x5 deniyor (enerji ve bekleme yine engelliyor) |
| `EARLY BURST` | Rakipler ilk çeyrekte yükleniyor, sonra duruyor |

| `RUBBER BAND` | Ne yapıyor |
|---|---|
| `ON` / `OFF` | Dengeleme sistemini açıp kapatıyor |

Seçilen mod rakibin kararının üstüne biniyor: karar yine kendi mantığından çıkıyor
(STATS ekranında ne düşündüğü okunabilsin diye), sonra moda göre eziliyor.

### Hızlanmanın kuralı

Hiç basmayan araba saniyede **16 metre** gidiyor. Kabul edilen bir basış **1 saniyelik**
bir pencere açıyor ve o pencere boyunca hız `k` katına çıkıyor. Yani o saniyede alınan
yol `k × 16` metre, normale göre kâr `(k−1) × 16` metre.

| Buton | 1 saniyede gidilen yol | Fazladan | Fiyat | Metre / enerji | Hız şekli |
|---|---|---|---|---|---|
| 1 | 16 m | 0 m | 0 | – | Düz |
| 2 | 32 m | 16 m | 14 | 1.143 | Yumuşak |
| 3 | 48 m | 32 m | 29 | 1.103 | **Yumruk** |
| 4 | 64 m | 48 m | 44 | 1.091 | **İki kademe** |
| 5 | 80 m | 64 m | 60 | 1.067 | **Final patlaması** |

### Beş buton neden farklı hissettiriyor

Butonlar birbirinin büyüğü olmasın diye her biri kendi mesafesini **farklı bir hız
şekliyle** veriyor. GDD buna zaten izin veriyor: *"Görsel ivmelenme/yavaşlama
kullanabilirsiniz; hız eğrisinin toplam mesafesi bu sözleşmeyi korumalıdır."*
Toplam mesafe hepsinde tıpatıp aynı, değişen tek şey o mesafenin 1 saniyenin neresine
düştüğü.

Hesap şöyle işliyor: pencere boyunca hız, taban hızın üstüne şeklin o anki değeri kadar
ekleniyor. Şeklin altında kalan alan hep 1'e eşit olacak şekilde ayarlı, dolayısıyla
toplam mesafe şekilden bağımsız olarak hep `k × 16` metre çıkıyor. Her adımın mesafesi
formülle hesaplanıyor, tek tek toplanmadığı için hata birikmiyor.

| Şekil | Karakteri | İlk yarım saniyeye düşen pay | Ne zaman iyi |
|---|---|---|---|
| Düz | Sabit | %50 | Referans |
| Yumuşak | Yavaşça açılıp kapanıyor | %50 | Tempo tutmak |
| Yumruk | Anında fırlatıp sönüyor | **%64.6** | Öndekini **hemen** yakalamak |
| İki kademe | İki kez itiyor, ortada nefes | %50 | Uzun düzlükte |
| Final patlaması | Yavaş başlıyor, sonda patlıyor | **%36.6** | **Çizgiye koşarken** |

Bu yüzdeler tahmin değil, `buff_rules.csv` içinde ölçülüyor. Sonuç olarak x3 aradaki
boşluğu anında kapatıyor (saniyede 64 metreyle açılıp 16'ya iniyor), x5 ise sona doğru
patladığı için bitiş çizgisinde daha iyi iş görüyor. Soru "kaça bassam" değil, "şu an
hangisi işime yarar" oluyor.

Rakipler de bunu biliyor: karar verirken her butonun açılıştaki gücünü hesaba katıyorlar.
Öndekini yakalamak isteyen Yumruk'a, finalde bağlanacak olan Final patlamasına meylediyor.
Ayrı bir tablo yazmadım, doğrudan şeklin kendisinden çıkıyor.

Aynı anda tek hızlanma çalışıyor. Süresi dolmadan gelen basış **yok sayılıyor**: süre
uzamıyor, hızlar toplanmıyor, sıraya girmiyor. Kabul edilmeyen basış ne hız veriyor ne
enerji harcıyor. Geri sayımda ve yarış bittikten sonra gelen basışlar `NotRunning` ile
geri çevriliyor. Yeniden başlatma her şeyi sıfırlıyor: açık pencereler, enerji, bekleme
süreleri ve rakiplerin bekleyen kararları.

### Enerji, bekleme ve dolum

Tek kaynak var: **enerji**. En fazla 100 tutuyor, yarışa 75 ile başlıyorsun, saniyede 5
doluyor. Bir hızlanma bitince 0.35 saniye beklemek gerekiyor.

**Neden böyle.** Fiyatlar kârdan biraz daha hızlı artıyor, dolayısıyla metre başına enerji
verimi x2'den x5'e doğru %7 düşüyor. Bu iki şeyi birden sağlıyor: güçlü hamlenin gerçek bir
bedeli var ama sürekli x5 basmak da kazandıran bir yol değil, sadece **en israflı** yol.
Buna karşılık x5 tek seferde 64 metre kazandırdığı için pozisyon almanın en hızlı yolu.
Böylece soru "hangi buton" değil, "şimdi mi sonra mı" oluyor.

İkinci bedel daha sessiz ama daha ağır: enerji tavana dayandıktan sonra dolan enerji çöpe
gidiyor. Saniyede 5 enerji = saniyede 5.7 metre kayıp, yani hiç basmamak da en az yanlış
basmak kadar pahalı. Testlerde hiç basmayan oyuncu **381 enerji** çöpe atıyor.

### Basış kabul edilmezse

Sebebi ekranın ortasında yazıyor: `BOOST ALREADY RUNNING`, `COOLING DOWN`,
`NOT ENOUGH ENERGY`, `RACE NOT RUNNING`. Ayrıca butonlar da durumu gösteriyor:
basılabilenler açık, parası yetmeyenler ve bekleme sırasındakiler sönük, çalışan buton
kendi renginde. Kabul edilen basışta kutu ve alt bar o butonun rengine dönüyor, üstte
adı ve kalan süre yazıyor (`x3 PUNCH 0.68s`).

Hız şekilleri ekranda da görünüyor: arabanın arkasındaki izin kalınlığı, gövdenin
parlaması ve egzoz alevi o anki hıza göre değişiyor, kamera da öyle. Yumruk'ta iz
açılışta patlayıp sönüyor, İki kademede iki kez şişiyor, Final patlamasında sona doğru
büyüyor.

---

## Görsel taraf

Çalışırken kodla hiçbir şey çizilmiyor. Bütün parçacıklar, yol modeli ve ekran efektleri
editörde önceden hazırlanıp diske prefab olarak hazırlandı. Oyun sadece bu hazır
parçaları yüklüyor. Yani bir parçacığı değiştirmek, yerini kaydırmak ya da paketteki
başka bir efekti takmak için prefabı açman yeterli, kod üstüne yazmıyor.

| Hazırlanan dosya | Nereden geldi |
|---|---|
| `Prefabs/Effects/Fx_Thruster` | ParticlePack ▸ FlameStream |
| `Prefabs/Effects/Fx_RoadDust` | ParticlePack ▸ SmokeEffect |
| `Prefabs/Effects/Fx_BoostSparks` | ParticlePack ▸ Legacy SparksEffect |
| `Prefabs/Effects/Fx_Shockwave` | ParticlePack ▸ TinyExplosion |
| `Prefabs/Effects/Fx_Confetti` | ParticlePack ▸ Misc SparksEffect |
| `Prefabs/Effects/Fx_BoostFlare` | nokta ışık |
| `Prefabs/Track/RaceTrack` | yol modeli + 79 dekor + bitiş kapısı |
| `Art/Models/Generated/RaceRoad` | yol modeli |
| `Settings/RaceVolumeProfile` | ekran efektleri ayarı |

**Her arabada** (oyuncu ve yedi rakip, hepsi aynı düzen): egzozda alev, hızlanınca
seviyeye göre büyüyen kıvılcım patlaması, yere yayılan halka, hıza göre koyulaşan yol
tozu, hızlanma boyunca yanan bir ışık ve bitiş çizgisini geçince kutlama patlaması.
Hepsi araba prefabının `Effects` klasöründe duruyor. Kod sadece rengi, parlaklığı ve
patlama büyüklüğünü ayarlıyor; parçacığın ömrü, hızı ve materyali prefabda yazıyor.
Parçacıklar dünyaya bırakılıyor, yani araba fırlarken alev arkada kalıyor.

**Kamera** üçe ayrıldı: `CameraRig` takip ve görüş açısını, `CameraShake` sarsıntıyı,
`ScreenEffects` ekran efektlerini yönetiyor. Kabul edilen her hamle seviyesine göre bir
sarsıntı ve görüş açısı darbesi üretiyor, kamera yanal harekette hafifçe yatıyor.

**Ekran efektleri** sahnedeki `Race Camera ▸ RaceVolume` üzerinden geliyor, ayarları
`Settings/RaceVolumeProfile` dosyasında: Bloom, Vignette, renk kayması, renk ayarı ve
hareket bulanıklığı. Hepsinin şiddeti tek bir değerden çıkıyor (hız fazlası + hamle
darbesi), yani sakin sürüşte neredeyse yok, x5'te tavanda.

**HUD** hızı **km/sa** gösteriyor (16 m/sn ≈ 58 km/sa, x5'te ≈ 288 km/sa) ve sayı hedefe
yumuşayarak gidiyor, hızlandıkça rengi ısınıyor. Kabul edilen hamlede ilgili buton
zıplıyor, enerji barı beyaz çakıyor; enerji %30'un altına inince bar kırmızı yanıp
sönüyor. Sıralama değişince kart zıplayıp yeşil (çıkış) ya da kırmızı (düşüş) parlıyor.
STATS ekranı kayıt dosyalarıyla aynı kalsın diye metre/saniye gösteriyor.

---

## Yarış ve rakipler

### Nasıl hareket ediyorlar

Fizik yok. Her araba için tek bir "ne kadar yol gitti" değeri tutuluyor, pist de bu
değeri yol üzerindeki bir noktaya çeviriyor. Şeritler sadece görüntü için: sekiz araba
yan yana ayrı çizgilerde gidiyor ama sıralama her zaman yolun ortasından ölçülen gerçek
mesafeye göre yapılıyor.

Hesap saniyede **120 kez**, hep aynı büyüklükteki adımlarla yürüyor. Hız bir adım
içinde sabit kalıyor ve hızlanma penceresi bittiği anda adım tam oradan ikiye bölünüyor.
Bunun iki faydası var: pencerede alınan yol adım büyüklüğünden bağımsız olarak tam
`k × 16` metre çıkıyor, ve bitiş çizgisini tam olarak hangi anda geçtiğini hesaplamak
mümkün oluyor.

Bitiş sırası listedeki sıraya göre değil, bu geçiş anına göre veriliyor. Aynı adımda
birden fazla araba geçerse önce hepsi toplanıyor, geçiş anına göre sıralanıyor, sonra
sıra dağıtılıyor.

### Rakip karakterleri

Yedi rakip de oyuncuyla **aynı kuralları** kullanıyor: aynı 1 saniye, aynı bekleme,
aynı fiyatlar. Ayrıldıkları yer hızları ve enerjiyi harcama tarzları. Böyle yapmamın
sebebi şu: rakibin yaptığı hamle senin de yapabileceğin bir hamle, dolayısıyla ekranda
olan biten anlaşılıyor.

| Karakter | Tarzı | Hızı | Kaç saniyede bir karar | Sevdiği buton | Sakladığı enerji |
|---|---|---|---|---|---|
| Striker | Fırsatçı | 1.031 | 3.7 | 4 | 34 |
| Closer | Finalde risk alan | 1.003 | 2.9 | 5 | 50 |
| Metronome | Hep aynı ritim | 0.998 | 1.9 | 3 | 0 |
| Defender | Arkadan gelene tepki veren | 0.990 | 2.3 | 3 | 10 |
| Erratic | Dengesiz | 1.018 | 4.1 | 4 | 16 |
| Drumbeat | Yavaş Metronome | 0.970 | 3.1 | 3 | 12 |
| Ambusher | Pusuda bekleyen | 0.985 | 4.3 | 4 | 26 |

### Nasıl karar veriyorlar

Rakip belli aralıklarla durup hem beklemeyi hem de parası yeten her butonu puanlıyor:

- **Yakalama** — öndeki araba tam olarak bir hızlanma kadar uzaktaysa puan yükseliyor
- **Savunma** — arkadaki araba yaklaştıkça yükseliyor
- **Tempo** — kendi hedef temposunun gerisine düştükçe yükseliyor (oyuncuyla ilgisi yok)
- **Final** — yarışın son bölümüne girdikçe yükseliyor
- **Taşma** — enerji tavana yaklaştıkça yükseliyor, israfı engelliyor
- **Verim** — o butonun metre başına enerji verimi
- **Karakter ve şans** — profilin eğilimi ve biraz rastgelelik

En yüksek puanı alan seçiliyor, beklemek de puanlanan bir seçenek. Bu puanların o anki
değerleri STATS ekranında araba satırında yazıyor, yani rakibin neden bastığı görülebiliyor.

Kararlar tam sayı adım sayaçlarıyla planlanıyor, her karakterin başlangıç zamanı kaydırılmış
ve karar aralıkları birbirine bölünmeyen sayılardan seçilmiş. Sıra her adımda dönüyor ve
iki rakip birbirinden yarım saniye geçmeden hızlanmaya başlayamıyor. Bunlar yedi rakibin
aynı anda atağa kalkmasını baştan engelliyor.

### Yarışı yakın tutma

Dengeleme, karar mantığından **tamamen ayrı** bir katman: sadece hızı azıcık artırıp
azaltıyor. Oyuncuya hiç uygulanmıyor, kayıtlarda oyuncunun aldığı katkı her yarışta tam
olarak 0.

- **Sessiz bölge:** oyuncuya 120 metreden yakın hiçbir arabaya dokunmuyor. Yani
  kapışmanın olduğu yerde sistem tamamen kapalı; ne sollama üretebiliyor ne engelleyebiliyor.
- **Yardım:** oyuncunun 120 metreden fazla gerisinde **ve** sırası oyuncudan kötü olan
  araba, 300 metrede %4'e çıkan bir hız artışı alıyor.
- **Frenleme:** lider bir rakipse ve ikinciyle arası 180 metreyi geçtiyse, 400 metrede
  %3'e inen bir yavaşlama alıyor. Oyuncu lidersse hiç çalışmıyor.
- **Sınırlar:** bu oran saniyede en fazla 0.06 değişiyor, hızlanma sırasında hiç
  değişmiyor, araba yolun %80'ini geçtikten sonra azalmaya başlıyor ve %95'te tamamen
  kapanıyor. Bir arabaya toplamda en fazla 45 metre yazılabiliyor.
- **Pasif oyuncu kuralı:** oyuncu sonuncuysa ve elindeki enerjinin %30'undan azını
  harcadıysa sistem tamamen kapanıyor. Kötü oynamak telafi edilmiyor.

Ölçülen sonuç: 12 yarışın hiçbirinde bir arabaya yazılan katkı **8.09 metreyi** geçmedi
(1300 metrelik pistin binde altısı, 45 metrelik sınırın beşte biri). Oyuncuya yazılan
**0.00 metre**.

### Oyuncunun avantajı neden korunuyor

Üç şey birlikte: dengeleme oyuncuya hiç uygulanmıyor, oyuncuya yakın rakiplere de
uygulanmıyor ve rakipler kararlarını oyuncunun nerede olduğuna değil kendi hedeflerine
göre veriyor. Rakip sana yaklaşıyorsa enerjisini iyi kullandığı için yaklaşıyor, sen
öne geçtin diye arkadan açılan gizli bir hız yardımı yok.

### Hiç basmayan ve sürekli x5 basan oyuncu

Ölçülmüş hâliyle: hiç basmayan oyuncu 81.25 saniyeyle sonuncu, birinciyle arası 370–408
metre ve 381 enerjiyi çöpe atıyor. Sürekli x5 basan oyuncu 57.89 saniyeyle yarışta
kalıyor ama basışlarının **621–632'si geri çevriliyor** ve ortalamada dengeli oynayanın
(57.03–58.33 sn) arkasında bitiriyor. Başta yüklenip sonra bırakan oyuncu 73.25 saniyeyle
sonuncu.

---

## Ayarlar

### Nerede duruyor

Her şey tek dosyada: **`Assets/Data/RaceConfig.asset`**. İçindeki bölümler:

| Bölüm | Ne ayarlanıyor |
|---|---|
| `race` | taban hız, geri sayım, adım büyüklüğü, seed, hız ve enerji dağılımındaki değişkenlik |
| `track` | yarış mesafesi, yol genişliği, şerit aralığı, yolun şekli, dekorlar |
| `boost` | pencere süresi, bekleme, enerji tavanı, başlangıç enerjisi, dolum, fiyatlar, renkler |
| `ai` | rakiplerin alt sınırı, puan ağırlıkları, sakladıkları enerji, aynı anda atak engeli |
| `balance` | sessiz bölge, yardım ve frenleme eşikleri, değişim hızı, kapanma noktası, metre sınırı |
| `telemetry` | kayıt sıklığı, klasör, dosya biçimleri |
| `player` / `rivals` | arabanın modeli, rengi, adı ve her rakibin karakteri |

Yarış kodunda elle yazılmış tek bir sayı yok; hepsi ya bu dosyada ya da ilgili bölümün
`Constants` dosyasında.

### En çok işe yarayan ayarlar

| Ayar | Etkisi |
|---|---|
| `boost.levelCosts` | Butonlar arasındaki verim farkı. Fiyatları düz artırmak hepsini eşitler, çok dikleştirmek üst butonları öldürür. |
| `boost.levelCurves` | Hangi butonun nasıl hızlandıracağı. Hepsini düz yapmak kuralı bozmaz, sadece beş butonu aynılaştırır. |
| `boost.punchSharpness` / `surgeSharpness` / `twoStepDepth` | Şekillerin sertliği. Yumruk 1.5'te saniyede 64 metreyle açılıyor; 2.5 yapınca 96 oluyor, toplam mesafe yine değişmiyor. |
| `boost.energyRegenPerSecond` | Yarış boyunca eline geçen toplam enerji, yani iyi oyunla kötü oyun arasındaki fark. |
| `boost.cooldown` | Basma ritmi. 1.35 saniyeden sık basmaya çalışan her istek kesin geri çevriliyor. |
| `AiProfile.speedScale` | En çok etkiyi yapan ayar: %1 hız ≈ 0.6 saniye ≈ 14 metre. Rakip sıralamasını bununla kurdum. |
| `AiProfile.holdBias` / `reserveEnergy` | Rakibin ne kadar biriktirdiği. Çok biriktiren pusucu, hiç biriktirmeyen metronom oluyor. |
| `balance.muteBand` | Dengelemenin oyuncuya ne kadar yaklaşabildiği. 120 metrenin altına inince oyuncunun kararlarını yemeye başlıyor. |
| `race.speedScaleJitter` | Yarıştan yarışa değişkenlik. 0'da her yarış aynı sırayla bitiyor, %2'de sıralama seed'e göre değişiyor. |

---

## Testler

Bütün sayılar `Docs/Validation/` altında üretiliyor: Unity'de
`Tools > BrokRace > Run Race Validation` menüsünden, komut satırında
`Editor.RaceValidationRunner.Run` ile. Testler oyunun **kendi yarış kodunu** çalıştırıyor,
sadece ekran çizmiyor. Yani bu sayılar ayrı bir hesaplamadan değil, oyunun kendisinden
geliyor.

| Dosya | İçinde ne var |
|---|---|
| `buff_contract.csv` | Her buton ve şekil için, dört ayrı adım büyüklüğünde ölçülmüş mesafe |
| `buff_rules.csv` | 9 kural kontrolü |
| `lifecycle.csv` | Bitişe denk gelen hızlanma, aynı anda çizgi geçme, arka arkaya restart |
| `frame_rate.csv` | 30/60/120 FPS karşılaştırması |
| `race_matrix.csv` | 12 yarışın oyuncu özeti |
| `race_cars.csv` | 12 yarışın sekiz arabasının hepsi |
| `<senaryo>_<seed>_samples.csv` | Saniyede 10 kez kayıt (mesafe, hız, buton, enerji, dengeleme, rakip kararı) |
| `<senaryo>_<seed>_events.csv` | Kabul, ret, sollama ve bitiş anları |
| `<senaryo>_<seed>_report.json` | Yarış özeti |

### 12 yarışın özeti

Seed'ler: 1337, 90210, 777013. Seed, yarıştaki rastgeleliği belirleyen numara; aynı
numarayla yarış hep aynı çıkıyor. Oyuncunun basışları elle değil, önceden hazırlanmış
saatli bir listeden geliyor.

| Senaryo | Seed | Süre | Sıra | Kabul | Ret | Harcanan | İsraf | Hızlanma mesafesi | Sollama +/− | Son %20 öndeki | Son %20 lidere |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Dengeli | 1337 | 57.07 | **1** | 17 | 1 | 359 | 0 | 387.0 | 5 / 5 | 0.0 | 0.0 |
| Dengeli | 90210 | 58.29 | 4 | 14 | 2 | 333 | 0 | 367.5 | 3 / 6 | 14.6 | 50.6 |
| Dengeli | 777013 | 57.25 | **1** | 12 | 1 | 349 | 0 | 384.0 | 6 / 6 | 1.4 | 1.4 |
| Hiç basmayan | 1337 | 81.25 | 8 | 0 | 0 | 0 | 381 | 0.0 | 0 / 7 | 297.7 | 370.0 |
| Hiç basmayan | 90210 | 81.25 | 8 | 0 | 0 | 0 | 381 | 0.0 | 0 / 7 | 248.7 | 408.1 |
| Hiç basmayan | 777013 | 81.25 | 8 | 0 | 0 | 0 | 381 | 0.0 | 0 / 7 | 308.5 | 372.1 |
| Sürekli x5 | 1337 | 57.89 | **1** | 6 | 627 | 360 | 0 | 374.4 | 11 / 11 | 2.3 | 5.0 |
| Sürekli x5 | 90210 | 57.89 | 4 | 6 | 634 | 360 | 0 | 374.4 | 5 / 8 | 14.6 | 38.4 |
| Sürekli x5 | 777013 | 57.89 | 3 | 6 | 631 | 360 | 0 | 374.4 | 11 / 14 | 18.2 | 25.7 |
| Erken atak + pasif | 1337 | 73.25 | 8 | 2 | 13 | 120 | 221 | 128.0 | 2 / 9 | 150.6 | 247.5 |
| Erken atak + pasif | 90210 | 73.25 | 8 | 2 | 13 | 120 | 221 | 128.0 | 1 / 8 | 150.4 | 278.2 |
| Erken atak + pasif | 777013 | 73.25 | 8 | 2 | 13 | 120 | 221 | 128.0 | 1 / 8 | 172.6 | 266.9 |

**Ne anlama geliyor.** Sıralama oyun kalitesini takip ediyor: dengeli oynayan 1/4/1,
sürekli x5 basan 1/4/3, başta yüklenip bırakan 8, hiç basmayan 8. Dengeli oyun ortalama
57.5 saniye, sürekli x5 ise 57.89 — aradaki 0.4 saniye tam olarak verim farkının
karşılığı. Yani sürekli x5 oynanabilir ama en iyisi değil. Dengeli yarışlarda hiç israf
yok; iyi oyunun tanımı "enerjiyi tavanda bekletmemek" olmuş durumda.

Rekabet tarafı: altı kapışmalı yarışta **üç ayrı kazanan** çıktı (oyuncu 3, Metronome 2,
Closer 1), ilk üç arasındaki fark **0.74–1.80 saniye**, sekiz arabanın bitiş aralığı
4.6–8.0 saniye, yarış başına 3–13 kez lider değişiyor. Karakterlerin ortalama bitiş
sırası Metronome 2.2, Defender 2.8, Closer 2.9, Striker 4.1, Drumbeat 5.7, Erratic 6.1,
Ambusher 7.0 — sıralama duruyor ama hiçbiri sabit değil. 12 yarışın tamamına bakınca
Defender ve Striker de birinciliği görüyor. Yedi karakterin hepsinde en iyi ve en kötü
derece arasında en az üç sıra fark var, yani sonuç gerçekten seed'e göre değişiyor.

Butonlara farklı şekil vermek rekabeti sıkılaştırdı: ilk üç arasındaki fark 1.14–2.01
saniyeden 0.74–1.80 saniyeye indi, çünkü rakipler artık duruma uygun şekli seçiyor ve
sonda Final patlamasına bağlananlar gerçekten yetişiyor.

### Hızlanma ölçümü

`buff_contract.csv` — her buton, saniyede 30 / 60 / 120 ve kasten bölünmeyen 37 adımda
ölçüldü. Ölçüm sayacı değil, arabanın gerçekten aldığı yolu okuyor.

| Buton | Şekil | Olması gereken | Ölçülen | En büyük sapma |
|---|---|---|---|---|
| 1 | Düz | 16 m | 16 m | %0.000060 |
| 2 | Yumuşak | 32 m | 32 m | %0.000060 |
| 3 | Yumruk | 48 m | 48 m | %0.000056 |
| 4 | İki kademe | 64 m | 64 m | %0.000042 |
| 5 | Final patlaması | 80 m | 80 m | %0.000038 |

Hedef %1'di, çıkan sapma bunun binde biri bile değil.

`buff_rules.csv` — dokuzu da PASS:

1. Pencere doluyken gelen basış yok sayılıyor (buton değişmiyor, enerji gitmiyor, mesafe bozulmuyor)
2. Bekleme sırasındaki basış bedava geri çevriliyor, süre bitince kabul ediliyor
3. Enerji yetmiyorsa bedava geri çevriliyor
4. Geri sayımda ve bitişten sonra basış geçmiyor
5. x1, hiç basmamış bir arabayla aynı yolu alıyor
6. Aralık dışındaki değerler 1–5'e çekiliyor
7. Bitiş çizgisini geçme anı tam hesaplanıyor (bulunan 4.00417 sn, olması gereken
   4.00417 sn, adım 0.00833 sn). Hızlanma sırasında hız adım içinde sabit olmadığı için
   bu an ayrıca çözülüyor
8. **Beş şeklin hepsi aynı mesafeyi veriyor** — en büyük sapma %0.000103
9. **Şekiller mesafeyi pencere içinde gerçekten kaydırıyor** — ilk yarıya düşen pay
   Yumruk %64.6, Düz %50.0, Final patlaması %36.6

Tuşu basılı tutmak kaynağında tek istek üretiyor.

### FPS farkı

`frame_rate.csv` — aynı seed, aynı saatli girdi, 120/60/30 FPS:

| Seed | FPS | Süre | Sıra | Hızlanma mesafesi | Fark |
|---|---|---|---|---|---|
| 1337 | 120 / 60 / 30 | 57.074 sn | 1 | 386.963 m | %0.0 |
| 90210 | 120 / 60 / 30 | 58.289 sn | 4 | 367.476 m | %0.0 |
| 777013 | 120 / 60 / 30 | 57.253 sn | 1 | 383.999 m | %0.0 |

Fark yaklaşık değil, tam sıfır. Sebebi şu: geçen süre bir kenarda toplanıyor ve hesap her
zaman aynı büyüklükte adımlarla yürüyor. 30 FPS'te bir karede 4 adım, 60'ta 2, 120'de 1
adım işleniyor ve **adımların sırası birebir aynı kalıyor**. Bu, ekranın hızına değil
hesabın kendi saatine bağlı bir şey. Böyle yazdım çünkü canlı klavyeyle oynarken basış
anı en yakın kareye yuvarlanıyor ve o kadarlık fark kalıyor. Saatli girdiyle sonuç
harfiyen aynı.

### Bitiş ve restart

`lifecycle.csv` — üçü de PASS:

1. Bitişe denk gelen hızlanma: çizgi bir kez geçiliyor, geçerken pencere hâlâ açık ve
   64 metrenin tamamı verilmiş
2. Aynı adımda iki geçiş: önce raporlanan araba 0 iken kazanan araba 1 — çünkü 5.25
   milisaniye önce geçmiş. Listedeki sıra sonucu belirlemiyor
3. Arka arkaya restart: aynı seed'le iki yarış 57.074 sn / 1. sıra / 386.96 m ile tıpatıp aynı

### Mesafe–zaman grafiği

`Docs/Validation/<senaryo>_<seed>_samples.csv` saniyede 10 kez sekiz arabanın da mesafesini
yazıyor; `time` ve `distance` sütunlarını alıp direkt grafik çizebilirsin. `gapToPlayer`
ve `gapToLeader` sütunları araların nasıl açılıp kapandığını, `aiState` sütunu rakibin o
an neye baktığını gösteriyor.

---

## Kod düzeni

Tek bir `Update` var: `GameManager` durum makinesini çalıştırıyor. Onun altında iki saat
dönüyor — sabit adımlı **hesap** ve kare başına bir çalışan **görüntü**. Görüntü tarafı
durumu sadece okuyor, asla değiştirmiyor.

```
Assets/Scripts/
  Core/            DI, durum makinesi, UI tabanı, tween, rastgelelik
  Game/
    Boot/          servisleri sabit sırayla kaydeden tek başlatıcı
    Configuration/ RaceConfig ve içindeki ayar blokları
    Race/          RaceLoop (yarışın tamamı) + RaceManager (sahne tarafı)
    Cars/          Car, CarMotion, CarVisual
    Boost/         BoostController, hız şekilleri ve enerji hesabı
    Ai/            AiDriver, aynı anda atak engeli
    Balancing/     RubberBandBalancer
    Standings/     sıralama ve bitiş sırası
    Track/         hazır pisti yükleyen katman ve yolun eğrisi
    Effects/       efektleri ve ekran efektlerini yöneten kısım
    Telemetry/     kayıt ve dosya yazma
    Simulation/    ekransız yarış ve kural testleri
    Input/ UI/ States/ Camera/
```

İşler dar arayüzlere bölünmüş: `ICar` hesap, `ICarView` görüntü, `IBoostController` komut,
`IBoostState` sorgu, `IBoostClock` zaman, `IBoostLedger` sayaç. Bir hata ararken bakılacak
tek yer var: mesafe `CarMotion.Integrate`, hız şekli `BoostCurves`, pencere
`BoostController`, rakip kararı `AiDriver.Choose`, dengeleme `RubberBandBalancer.Target`,
sıralama `StandingsManager.Compare`.

Yarışın kendisi `RaceLoop` adında sade bir sınıf. Sahne onu bir MonoBehaviour üzerinden
çalıştırıyor, testler ise hiç GameObject olmadan aynı sınıfı çalıştırıyor.

---

## Teslim notları

**Video:** `Build/Gameplay.mp4` — build üzerinden alınmış oynanış kaydı.

**Harcanan süre:** Üç günde toplam 20 saat civarı. Büyük kısmı ilk gün gitti (yarış
mantığı, rakipler, testler), kalanı görsel taraf ve denge ayarı.

**Kullanılan üçüncü taraf araçlar:**

| Ne | Ne için kullandım |
|---|---|
| UniTask | Bekleme ve zamanlama işleri. Coroutine kullanmadım. |
| Addressables | Araba, efekt ve pist parçalarını gerektiğinde yükleyip işi bitince bırakmak. |
| Input System | Ekrandaki butonların girdisi. |
| PoolManager | Kendi yazdığım paket, nesneleri tekrar tekrar kullanmak için. |
| DIContainer | Kendi yazdığım paket, dependecy injection için kullandım. |
| FSM | Kendi yazdığım paket, oyun içindeki state machineleri generic kontrol eden yapı. |
| Unity Particle Pack | Efektler: alev, toz, kıvılcım, konfeti. |
| Modular Cyber Racing Cars | Araba modelleri. |

**Kullanılan AI araçları:** Claude Code.AI Senaryolarının analizi, csv çıktı alt yapısı ve dokümanları toparlamakta kullandım. Kodlama, Tasarım ve denge
kararları bana ait.

## Bilinen eksikler

- Ses yok.
- Arabalar birbirine çarpmıyor. Şeritler görüntü için, sekiz araba yan yana ayrı
  çizgilerde gidiyor ve üst üste binmiyor. Virajda dıştaki şeridin biraz daha uzun yol
  gitmesini bilerek hesaba katmadım, mesafe her zaman yolun ortasından ölçülüyor.
- Klavyeyle canlı oynanan iki yarış birebir aynı olmaz, çünkü basış anı en yakın kareye
  yuvarlanıyor. Tam aynısını tekrarlamak saatli girdiyle mümkün ve bütün test sayıları
  oradan geliyor.
- Testler 3 seed × 4 senaryo. Rakip sıralamasını daha sağlam oturtmak için 30 seed'lik
  daha geniş bir tarama iyi olurdu.
- x1 bedava ama yine de 1.35 saniyelik beklemeyi getiriyor. Bunu bilerek yaptım, en basit
  butonun da bir bedeli olsun istedim.

## Bir gün daha olsa

- Rakiplerin hızlarını elle değil, hedeflenen bitiş süresine göre otomatik ayarlatırdım.
- Öndeki arabanın arkasına girince küçük bir hız kazancı (slipstream) eklerdim. Ekonomiye
  dokunmadan grup içi mücadeleyi renklendirirdi.
- Rakip atağa kalkmadan hemen önce küçük bir hazırlık işareti koyardım, ekranda daha
  okunur olurdu.
- Yarışı sonradan izleme. Kayıt dosyaları zaten bir yarışı baştan kurmaya yetecek bilgiyi
  tutuyor.
