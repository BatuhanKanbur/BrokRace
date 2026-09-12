# BrokRace

Tek pistli, 1 oyuncu + 7 AI'lı 3D yarış prototipi. Araç otomatik ilerler; oyuncu
direksiyon kullanmaz, üst sıradaki **1–5** tuşlarıyla hızlanma hamlesi yapar.

---

## Kurulum ve çalıştırma

- **Unity sürümü:** 6000.3.10f1
- **Render pipeline:** URP 17.3.0 (asset paketi Built-in için gelmişti, materyaller URP/Lit'e çevrilmiş durumda)
- **Açılacak sahne:** `Assets/Scenes/Race.unity` (tek sahne, build'de de tek sahne)
- **Build çalıştırma yolu:** `Build/BrokRace/BrokRace.exe` — açılışta doğrudan yarışa girer
- **Ek bağımlılıklar:** UniTask, Addressables, Input System, kendi `PoolManager` paketim
  (`com.batuhankanbur.poolmanager`). Hepsi `Packages/manifest.json` içinde.

Projeyi klonlayıp açtığınızda Addressables içeriği `Assets/Settings/AddressableAssetsData`
altında hazırdır. Yeniden üretmek gerekirse `PlayerBuilder.BuildContent`.

---

## Kontroller ve buff/kaynak kuralları

### Tuşlar

| Tuş | Etki |
|---|---|
| 1–5 (üst sıra ve numpad) | Hızlanma isteği, k seviyesinde |
| F1 | Geliştirici görünümü aç/kapa |
| R | Yarış bittikten sonra yeniden başlat (ekrandaki butonla aynı) |

Her basış tek istek üretir; tuşu basılı tutmak tekrar üretmez (`wasPressedThisFrame`).

### Sözleşme

Girdisiz araç `v0` taban hızıyla ilerler. Kabul edilen bir `k` tuşu **1.00 saniyelik**
pencere açar ve o pencerede toplam hız `k × v0` olur. Yani pencere boyunca alınan yol
`k × v0 × 1 sn`, normale göre kazanç `(k−1) × v0`. `v0 = 16 m/sn` için:

| Tuş | 1 saniyede yol | Ek kazanç | Maliyet | m / enerji | Eğri |
|---|---|---|---|---|---|
| 1 | 16 m | 0 m | 0 | – | Flat |
| 2 | 32 m | 16 m | 14 | 1.143 | Smooth |
| 3 | 48 m | 32 m | 29 | 1.103 | **Punch** |
| 4 | 64 m | 48 m | 44 | 1.091 | **Two-step** |
| 5 | 80 m | 64 m | 60 | 1.067 | **Surge** |

### Teslimat eğrileri

Beş tuş sadece "aynı şeyin büyüğü" olmasın diye her seviye kendi mesafesini **farklı
bir hız eğrisiyle** teslim ediyor. GDD bunu açıkça serbest bırakıyor: *"Görsel
ivmelenme/yavaşlama kullanabilirsiniz; hız eğrisinin toplam mesafesi bu sözleşmeyi
korumalıdır."* Toplam mesafe bütün eğrilerde birebir aynı, değişen tek şey o mesafenin
saniye içinde nereye düştüğü.

Matematiksel olarak: pencerede anlık hız `v0 · (1 + (k−1)·f(t/T))`, burada `f`
normalize edilmiş bir şekil fonksiyonu (`∫₀¹f = 1`). Toplam mesafe
`v0·T + (k−1)·v0·T·∫₀¹f = k·v0·T` — şekilden bağımsız olarak **tam**. Alt adım
integrali kapalı formülle (`F`, `f`'in ters türevi) alınıyor, dolayısıyla sayısal
hata birikmiyor.

| Eğri | Karakter | Pencerenin ilk yarısına düşen pay | Ne zaman iyi |
|---|---|---|---|
| Flat | Sabit | %50 | Referans |
| Smooth | Yumuşak yükselip sönen | %50 | Tempo tutmak |
| Punch | Anında vuruş, sonra sönüm | **%64.6** | Öndekini **şimdi** yakalamak |
| Two-step | Çift vuruş, ortada nefes | %50 | Uzun düzlükte iki kademe |
| Surge | Yavaş başlar, sonda patlar | **%36.6** | **Çizgiye koşu**, geç bağlanma |

Bu yüzdeler tahmin değil, `buff_rules.csv` içinde ölçülüyor. Pratik sonucu: 3 tuşu
boşluğu anında kapatır (64 m/sn ile başlar, 16'ya iner), 5 tuşu ise sona doğru
patladığı için çizgiye koşuda daha iyidir. Karar artık "kaç bassam" değil, "hangi
karakter bu duruma uyuyor" sorusu.

Rakipler de bunu biliyor: `AiDriver` her seviyenin eğri ön-yükünü puanına katıyor,
yani öndekini yakalamak isteyen rakip Punch'a, finalde bağlanacak olan Surge'e meyleder.
Ayrı bir tablo değil, doğrudan eğrinin kendi integralinden türetiliyor.

Aynı anda tek etki çalışır. Pencere açıkken gelen istek **yok sayılır**: süre uzamaz,
çarpan toplanmaz, kuyruk tutulmaz. Reddedilen istek ne hız verir ne kaynak harcar.
Geri sayımda ve yarış bittikten sonra istekler `NotRunning` ile reddedilir. Yeniden
başlatma bütün pencereleri, kaynakları, bekleme sürelerini ve bekleyen AI kararlarını
sıfırlar.

### Maliyet, bekleme ve yenilenme

Tek kaynak: **enerji**. Kapasite 100, yarışa 75 ile başlanır, saniyede 5 yenilenir.
Pencere kapandıktan sonra 0.35 sn ortak bekleme süresi vardır.

**Gerekçe.** Maliyetler kazancın hafif üstünde artıyor, dolayısıyla metre başına enerji
verimi 2'den 5'e doğru %7 düşüyor. Bu iki şeyi birden sağlıyor: güçlü hamlenin gerçek
bir maliyeti var, ama sürekli 5 basmak da baskın strateji değil — sadece **en verimsiz**
seçenek. Buna karşılık 5, tek seferde 64 m'lik sıçrama verdiği için pozisyon almanın tek
hızlı yolu. Karar "hangi seviye" değil, "şimdi mi sonra mı" sorusu hâline geliyor.

İkinci maliyet daha sessiz ama daha sert: enerji tavanda beklerken yenilenen enerji
çöpe gider. Saniyede 5 enerji = saniyede 5.7 m kayıp, yani hiç basmamak kadar pahalı.
Doğrulama koşularında hiç basmayan oyuncu **381 enerji** israf ediyor.

### Ret durumlarının geri bildirimi

Ret nedeni ekranın ortasında yazıyla belirir: `BOOST ALREADY RUNNING`, `COOLING DOWN`,
`NOT ENOUGH ENERGY`, `RACE NOT RUNNING`. Ayrıca 1–5 tuş kutuları anlık durumu gösterir:
karşılanabilir seviyeler açık, karşılanamayanlar ve pencere/bekleme sırasındakiler sönük,
aktif seviye kendi renginde. Kabul edilen hamlede kutu ve alt bar seviye rengine döner
ve üstte seviyenin eğri adı yazar (`x3 PUNCH 0.68s`).

Eğriler ekranda da okunuyor: araçtan çıkan izin kalınlığı ve gövdenin parlaması anlık
hız çarpanını takip ediyor, kamera görüş açısı da öyle. Punch'ta iz açılışta patlayıp
söner, Two-step'te iki kez şişer, Surge'de sona doğru büyür. Ayrı efekt asseti yok;
görsel kimliği eğrinin kendisi üretiyor.

---

## Yarış ve AI yaklaşımı

### Hareket ve sıralama modeli

Fizik yok. Her aracın tek bir skaler `Distance` değeri var; pist bu mesafeyi dünya
pozisyonuna çeviren arc-length parametrik bir rota (Catmull-Rom). Şeritler tamamen
görsel: sekiz araç sekiz ayrı yanal ofsette durur, sıralama her zaman merkez hattı
üzerindeki gerçek mesafeye göre yapılır.

Mantık sabit **1/120 sn** alt adımlarıyla işler, tek bir merkezi FSM tick'inden sürülür.
Hız alt adım içinde parçalı sabittir ve her alt adım buff penceresinin bitiş anında tam
olarak ikiye bölünür. Bunun iki sonucu var: pencere mesafesi adım boyundan bağımsız
olarak tam `k × v0 × T`, ve bitiş çizgisi geçiş anı alt adım içinde kapalı formülle
çözülebiliyor.

Bitiş sırası liste sırasına göre değil, bu geçiş anına göre veriliyor. Bir alt adımda
birden fazla araç geçerse önce hepsi toplanır, geçiş anına göre sıralanır, sonra sıra
dağıtılır.

### Rakip profilleri

Yedi rakip de oyuncuyla **aynı buff sözleşmesini** kullanıyor: aynı pencere, aynı bekleme
süresi, aynı maliyet tablosu. Ayrıldıkları yer harcama politikaları ve doğal hızları.
Bu tercihin sebebi: rakip bir hamle yaptığında ekranda gördüğünüz şey sizin de
yapabileceğiniz bir şey, dolayısıyla okunabilir.

| Profil | Karakter | Doğal hız | Karar aralığı | Seviye eğilimi | Rezerv | Baskın terim |
|---|---|---|---|---|---|---|
| Striker | Fırsatçı | 1.031 | 3.7 sn | 4 | 34 | strike 1.20 |
| Closer | Finalde risk alan | 1.003 | 2.9 sn | 5 | 50 | closing 1.30 |
| Metronome | Ritim | 0.998 | 1.9 sn | 3 | 0 | pace 0.95 / verim 0.85 |
| Defender | Tepkisel | 0.990 | 2.3 sn | 3 | 10 | defend 1.25 |
| Erratic | Kararsız | 1.018 | 4.1 sn | 4 | 16 | noise 1.20 |
| Drumbeat | Ritim (yavaş ikiz) | 0.970 | 3.1 sn | 3 | 12 | pace 0.80 |
| Ambusher | Pusucu | 0.985 | 4.3 sn | 4 | 26 | strike 1.35 / hold 0.80 |

### Karar zamanlaması

Her rakip beklemeyi ve karşılayabildiği her seviyeyi adlandırılmış terimlerle puanlar:

- **strike** — öndeki araç tam olarak bir k seviyesi boostu kadar uzaktaysa 1'e yaklaşır
- **defend** — arkadaki araç yaklaştıkça artar
- **pace** — kendi hedef temposunun gerisine düştükçe artar (oyuncudan bağımsız)
- **closing** — yarışın kendi bitiş bölgesine girdikçe artar
- **spill** — enerjisi tavana yaklaştıkça artar, israfı engeller
- **efficiency** — o seviyenin metre/enerji verimi
- **levelFit** ve **noise** — profil eğilimi ve tohumlanmış gürültü

En yüksek puanlı eylem seçilir; beklemek de puanlanan bir eylemdir. Her terimin o anki
değeri F1 görünümünde araç satırında yazıyor, yani bir rakibin neden bastığı okunabiliyor.

Kararlar tam sayı alt adım sayaçlarıyla planlanır, profil başına sabit faz (slot/7) ve
asal karar aralıkları kullanılır, sürücü sırası her alt adımda döner ve iki rakip
birbirinden 0.25 sn içinde pencere açamaz. Bunlar yedi rakibin aynı anda atak yapmasını
yapısal olarak engelliyor.

### Yakınlığı koruma yöntemi ve sınırları

Dengeleme **karar modelinden ayrı** bir katman: sınırlı bir hız katsayısı. Oyuncuya asla
uygulanmaz, telemetride oyuncunun katkısı her koşuda tam olarak 0.

- **Sessiz bant:** oyuncuyla arası 120 m'nin altındaki hiçbir araca dokunulmaz. Yani
  çekişmenin yaşandığı bölge tamamen dengelemesiz; dengeleme bir sollamayı ne üretebilir
  ne engelleyebilir.
- **Yardım:** oyuncunun 120 m'den fazla gerisinde **ve** sırası oyuncudan kötü olan araç,
  300 m'de 1.04'e ulaşan bir katsayı alır.
- **Frenleme:** lider bir rakipse ve ikinciyle arası 180 m'yi geçtiyse, 400 m'de 0.97'ye
  inen bir katsayı alır. Oyuncu lidersse hiç çalışmaz.
- **Sınırlar:** katsayı saniyede en fazla 0.06 değişir, aktif pencere sırasında hiç
  değişmez, araç kendi ilerlemesinin %80'ini geçtikten sonra sönümlenir ve %95'te tamamen
  kapanır. Her araç için toplam katkı ±45 m ile sınırlıdır.
- **Pasif oyuncu kuralı:** oyuncu sonuncuysa ve mevcut enerjisinin %30'undan azını
  harcadıysa dengeleme tamamen kapanır. Kötü oyun telafi edilmez.

Ölçülen sonuç: 12 koşunun tamamında herhangi bir araca yazılan en yüksek dengeleme katkısı
**8.09 m** (1300 m'lik pistin %0.62'si, ±45 m bütçesinin beşte biri), oyuncuya yazılan
**0.00 m**.

### Oyuncu avantajını koruma

Üç mekanizma birlikte: dengelemenin sessiz bandı (avantaj 120 m'nin altındayken sisteme
hiç dokunulmuyor), dengelemenin oyuncuya hiç uygulanmaması, ve rakip kararlarının
oyuncunun mesafesine değil kendi hedef temposuna bakması. Rakip yaklaşıyorsa bunun sebebi
kendi enerjisini iyi kullanması, oyuncu öne geçtiği için açılan gizli bir katsayı değil.

### Pasif oyuncu ve sürekli 5 davranışı

Ölçülmüş hâliyle: hiç basmayan oyuncu 81.25 sn ile sonuncu, lidere 370–408 m fark veriyor
ve 381 enerji israf ediyor. Sürekli 5 basan oyuncu 57.85 sn ile yarışta kalıyor ama
**621–632 isteği reddediliyor** ve dengeli oynayan oyuncunun (57.03–58.33 sn) ortalamada
gerisinde kalıyor. Erken atak sonra pasif senaryosu 73.25 sn ile sonuncu.

---

## Tuning

### Yapılandırmanın yeri

Her şey tek asset: **`Assets/Data/RaceConfig.asset`**. İçinde gömülü bloklar:

| Blok | Ne ayarlanır |
|---|---|
| `race` | taban hız, geri sayım, mantık adımı, seed, hız/enerji dağılım gürültüsü |
| `track` | yarış mesafesi, yol genişliği, şerit aralığı, rota noktaları, dekor referansları |
| `boost` | pencere süresi, bekleme süresi, kapasite, başlangıç enerjisi, yenilenme, seviye maliyetleri, seviye renkleri |
| `ai` | rakiplerin alt seviye sınırı, terim ölçekleri, rezerv eşiği, hava sahası kapısı |
| `balance` | sessiz bant, yardım/frenleme eşikleri ve tavanları, değişim hızı, sönümleme, metre bütçesi |
| `telemetry` | örnekleme aralığı, çıktı klasörü, CSV/JSON anahtarları |
| `player` / `rivals` | araç prefabı, rengi, adı ve her rakip için kendi AI profili |

Yarış kodunda hiçbir sayı yok; hepsi ya bu asset'te ya da ilgili alt sistemin `Constants`
dosyasında.

### En etkili parametreler

| Parametre | Etkisi |
|---|---|
| `boost.levelCosts` | Seviyeler arası verim farkı. Maliyetleri doğrusallaştırmak bütün seviyeleri eşitler, dikleştirmek yüksek seviyeleri öldürür. |
| `boost.levelCurves` | Hangi tuşun hangi karakteri taşıdığı. Hepsini `Flat` yapmak sözleşmeyi bozmaz, sadece beş tuşu aynılaştırır. |
| `boost.punchSharpness` / `surgeSharpness` / `twoStepDepth` | Eğrilerin sertliği. Punch 1.5'te 64 m/sn ile açar; 2.5'e çıkarmak açılışı 96 m/sn yapar, toplam mesafe yine değişmez. |
| `boost.energyRegenPerSecond` | Yarış boyunca toplam dönüştürülebilir enerji, dolayısıyla iyi ve kötü oyun arasındaki fark. |
| `boost.cooldown` | Basma ritmi. 1.35 sn'nin altına inen karar aralıkları garantili ret üretir. |
| `AiProfile.speedScale` | En sert kaldıraç: %1 doğal hız ≈ 0.6 sn ≈ 14 m. Merdiveni bununla kuruyorum. |
| `AiProfile.holdBias` / `reserveEnergy` | Profilin ne kadar biriktirdiği. Yüksek rezerv + yüksek hold = pusucu; sıfır rezerv = metronom. |
| `balance.muteBand` | Dengelemenin oyuncuya ne kadar yaklaşabildiği. 120 m'nin altına indirmek oyuncu iradesini yemeye başlar. |
| `race.speedScaleJitter` | Seed'ler arası çeşitlilik. %0'da her yarış aynı sırayla biter, %2'de podyum tohuma göre değişir. |

---

## Doğrulama

Bütün sayılar `Docs/Validation/` altında, batch mode'da üretiliyor
(`Editor.RaceValidationRunner.Run`). Yarış mantığı oyunla **aynı sınıf**, sadece
görselsiz sürülüyor, yani bu sayılar paralel bir modelden değil oyunun kendisinden.

| Dosya | İçerik |
|---|---|
| `buff_contract.csv` | Her seviye ve eğri için 4 farklı mantık adımında ölçülmüş pencere mesafesi |
| `buff_rules.csv` | 9 sözleşme kuralı |
| `lifecycle.csv` | Bitişe denk gelen buff, aynı alt adımda çizgi geçişi, peş peşe restart |
| `frame_rate.csv` | 30/60/120 FPS karşılaştırması |
| `race_matrix.csv` | 12 yarışın oyuncu özeti |
| `race_cars.csv` | 12 yarışın sekiz aracının tamamı |
| `<senaryo>_<seed>_samples.csv` | 10 Hz ham örnekler (mesafe, hız, seviye, enerji, dengeleme, AI durumu) |
| `<senaryo>_<seed>_events.csv` | Kabul/ret/sollama/bitiş olayları |
| `<senaryo>_<seed>_report.json` | Koşu özeti |

### 12 koşunun özeti

Seed'ler: 1337, 90210, 777013. Girdiler zamanlanmış ve tohumlanmış, `ScenarioBuilder`
tarafından üretiliyor.

| Senaryo | Seed | Süre | Sıra | Kabul | Ret | Harcanan | İsraf | Buff mesafesi | Sollama +/− | Son %20 öndeki | Son %20 lidere |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Dengeli | 1337 | 57.07 | **1** | 17 | 1 | 359 | 0 | 387.0 | 5 / 5 | 0.0 | 0.0 |
| Dengeli | 90210 | 58.29 | 4 | 14 | 2 | 333 | 0 | 367.5 | 3 / 6 | 14.6 | 50.6 |
| Dengeli | 777013 | 57.25 | **1** | 12 | 1 | 349 | 0 | 384.0 | 6 / 6 | 1.4 | 1.4 |
| Hiç buff yok | 1337 | 81.25 | 8 | 0 | 0 | 0 | 381 | 0.0 | 0 / 7 | 297.7 | 370.0 |
| Hiç buff yok | 90210 | 81.25 | 8 | 0 | 0 | 0 | 381 | 0.0 | 0 / 7 | 248.7 | 408.1 |
| Hiç buff yok | 777013 | 81.25 | 8 | 0 | 0 | 0 | 381 | 0.0 | 0 / 7 | 308.5 | 372.1 |
| Sürekli 5 | 1337 | 57.89 | **1** | 6 | 627 | 360 | 0 | 374.4 | 11 / 11 | 2.3 | 5.0 |
| Sürekli 5 | 90210 | 57.89 | 4 | 6 | 634 | 360 | 0 | 374.4 | 5 / 8 | 14.6 | 38.4 |
| Sürekli 5 | 777013 | 57.89 | 3 | 6 | 631 | 360 | 0 | 374.4 | 11 / 14 | 18.2 | 25.7 |
| Erken atak + pasif | 1337 | 73.25 | 8 | 2 | 13 | 120 | 221 | 128.0 | 2 / 9 | 150.6 | 247.5 |
| Erken atak + pasif | 90210 | 73.25 | 8 | 2 | 13 | 120 | 221 | 128.0 | 1 / 8 | 150.4 | 278.2 |
| Erken atak + pasif | 777013 | 73.25 | 8 | 2 | 13 | 120 | 221 | 128.0 | 1 / 8 | 172.6 | 266.9 |

**Yorum.** Sıra oyun kalitesini takip ediyor: dengeli 1/4/1, sürekli 5 1/4/3, erken atak
8, hiç basmayan 8. Dengeli oyun ortalama 57.5 sn, sürekli 5 ise 57.89 sn — aradaki 0.4 sn
tam olarak verim farkının karşılığı, yani sürekli 5 oynanabilir ama en iyisi değil.
Dengeli koşularda israf sıfır; iyi oyunun tanımı "enerjiyi tavanda bekletmemek" hâline
gelmiş durumda.

Rekabet tarafı: altı çekişmeli koşuda **üç farklı kazanan** (oyuncu 3, Metronome 2,
Closer 1), podyum **0.74–1.80 sn** içinde kapanıyor, sekiz aracın bitiş aralığı
4.6–8.0 sn, lider değişimi koşu başına 3–13. Profillerin ortalama bitiş sırası
Metronome 2.2, Defender 2.8, Closer 2.9, Striker 4.1, Drumbeat 5.7, Erratic 6.1,
Ambusher 7.0 — merdiven duruyor ama hiçbiri sabit değil. Altı çekişmeli koşunun
kazananları oyuncu, Metronome ve Closer; 12 koşunun tamamına bakıldığında Defender ve
Striker de birinciliği görüyor. Yedi profilin hepsinde en iyi ve en kötü derece arasında
en az üç sıra fark var, yani sıralama tohuma göre gerçekten değişiyor.

Eğri çeşitlendirmesi rekabeti sıkılaştırdı: podyum farkı 1.14–2.01 sn'den 0.74–1.80 sn'ye
indi, çünkü rakipler artık duruma uygun eğriyi seçiyor ve son bölümde Surge'e bağlananlar
gerçekten yetişiyor.

### Buff hesabı

`buff_contract.csv` — her seviye, saniyede 30 / 60 / 120 ve kasıtlı olarak bölünmeyen
37 mantık adımında ölçüldü. Ölçüm `BoostDistance` sayacını değil aracın gerçekten aldığı
yolu okuyor.

| Tuş | Eğri | Beklenen pencere mesafesi | Ölçülen | En kötü sapma |
|---|---|---|---|---|
| 1 | Flat | 16 m | 16 m | %0.000060 |
| 2 | Smooth | 32 m | 32 m | %0.000060 |
| 3 | Punch | 48 m | 48 m | %0.000056 |
| 4 | Two-step | 64 m | 64 m | %0.000042 |
| 5 | Surge | 80 m | 80 m | %0.000038 |

Hedef %1 idi; en kötü sapma bunun dört kat altında bir büyüklük mertebesinde.

`buff_rules.csv` — dokuzu da PASS:

1. Pencere açıkken gelen istek yok sayılıyor (seviye değişmiyor, enerji harcanmıyor,
   pencere mesafesi bozulmuyor)
2. Bekleme süresi isteği maliyetsiz reddediyor, süre bitince kabul ediyor
3. Yetersiz enerji maliyetsiz reddediyor
4. Geri sayımda ve bitişten sonra `NotRunning`
5. 1 tuşu hiç basmamış bir araçla aynı yolu alıyor
6. Aralık dışı seviyeler 1–5'e kırpılıyor
7. Bitiş çizgisi geçiş anı alt adım içinde tam (çözülen 4.00417 sn, analitik 4.00417 sn,
   adım 0.00833 sn). Eğri aktifken hız alt adım içinde sabit olmadığından geçiş anı
   Newton ile çözülüyor
8. **Beş eğrinin tamamı aynı pencere mesafesini teslim ediyor** — en kötü sapma %0.000103
9. **Eğriler mesafeyi pencere içinde gerçekten kaydırıyor** — ilk yarıya düşen pay
   Punch %64.6, Flat %50.0, Surge %36.6

Tuşu basılı tutma `wasPressedThisFrame` ile kaynakta tek istek üretiyor.

### Zaman adımı

`frame_rate.csv` — aynı seed, aynı zamanlanmış girdi, 120/60/30 FPS:

| Seed | FPS | Süre | Sıra | Buff mesafesi | Sapma |
|---|---|---|---|---|---|
| 1337 | 120 / 60 / 30 | 57.074 sn | 1 | 386.963 m | %0.0 |
| 90210 | 120 / 60 / 30 | 58.289 sn | 4 | 367.476 m | %0.0 |
| 777013 | 120 / 60 / 30 | 57.253 sn | 1 | 383.999 m | %0.0 |

Sapma yaklaşık değil, tam sıfır. Sebebi: kare süresi bir biriktiriciye ekleniyor ve
mantık her zaman aynı sabit alt adımla ilerliyor, yani 30 FPS'te kare başına 4, 60'ta 2,
120'de 1 alt adım işleniyor ve **alt adım dizisi birebir aynı oluyor**. Determinizm duvar
saatinde değil mantık zamanında tanımlı; bunu böyle yazıyorum çünkü canlı klavyeyle
oynarken basış anı kare ızgarasına yuvarlanır ve o kadarlık fark kalır. Zamanlanmış
girdiyle bit düzeyinde aynı.

### Yaşam döngüsü

`lifecycle.csv` — hepsi PASS:

1. Bitişe denk gelen buff: çizgi bir kez geçiliyor, geçiş anında pencere hâlâ açık, 64 m'lik
   ek mesafenin tamamı teslim edilmiş
2. Aynı alt adımda iki geçiş: önce raporlanan araç 0 iken kazanan araç 1 — çünkü 5.25 ms
   daha erken geçmiş. Liste sırası sonucu belirlemiyor
3. Peş peşe restart: aynı seed'le iki koşu 57.074 sn / 1. sıra / 386.96 m ile birebir aynı

### Mesafe–zaman grafiği

`Docs/Validation/<senaryo>_<seed>_samples.csv` 10 Hz'de sekiz aracın mesafesini içeriyor;
`time` ve `distance` sütunları doğrudan grafiğe verilebilir. `gapToPlayer` ve `gapToLeader`
sütunları farkların seyrini, `aiState` sütunu o anki AI karar terimlerini taşıyor.

---

## Mimari

Tek `Update` var: `GameManager` FSM'i tickliyor. Onun altında iki saat çalışıyor —
sabit alt adımlı **mantık** ve kare başına bir kez çalışan **sunum**. Sunum simülasyon
durumunu okur, asla yazmaz.

```
Assets/Scripts/
  Core/            DI konteyneri, FSM, UI tabanı, tween, deterministik rastgele
  Game/
    Boot/          servisleri sabit sırayla kaydeden tek bootstrapper
    Configuration/ RaceConfig ve gömülü ayar blokları
    Race/          RaceLoop (düz sınıf, yarışın tamamı) + RaceManager (sahne cephesi)
    Cars/          Car cephesi, CarMotion, CarVisual
    Boost/         BoostController, teslimat eğrileri ve ekonomi yardımcıları
    Ai/            AiDriver, hava sahası kapısı
    Balancing/     RubberBandBalancer
    Standings/     sıralama ve bitiş sırası
    Track/         spline, yol mesh'i, dekor
    Telemetry/     kayıt ve dosya yazımı
    Simulation/    başsız yarış, sözleşme ve yaşam döngüsü probları
    Input/ UI/ States/ Camera/
```

Sorumluluklar dar arayüzlerle ayrılmış: `ICar` mantık, `ICarView` sunum, `IBoostController`
komut, `IBoostState` sorgu, `IBoostClock` zaman, `IBoostLedger` sayaç. Bir hatayı ararken
bakılacak yer tek: mesafe `CarMotion.Integrate`, eğri `BoostCurves`, pencere
`BoostController`, karar `AiDriver.Choose`, dengeleme `RubberBandBalancer.Target`,
sıra `StandingsManager.Compare`.

Yarışın kendisi `RaceLoop` adında düz bir sınıf; sahne onu MonoBehaviour cephesiyle,
doğrulama koşuları da aynı sınıfı GameObject olmadan sürüyor.

---

## Bilinen eksikler

- Ses yok.
- Rakipler arası çarpışma yok; şeritler görsel, sekiz araç sekiz ayrı ofsette duruyor ve
  asla üst üste binmiyor. Viraj dışındaki şeridin gerçekte biraz daha uzun yol kat etmesi
  bilinçli olarak yok sayılıyor, resmi mesafe her zaman merkez hattı.
- Canlı klavyeyle oynanan iki koşu birebir aynı olmaz; basış anı kare ızgarasına yuvarlanır.
  Zamanlanmış girdiyle tam determinizm var ve bütün doğrulama sayıları oradan geliyor.
- Doğrulama 3 seed × 4 senaryo; profil merdivenini daha sağlam oturtmak için 30 seed'lik
  bir tarama daha iyi olurdu.
- 1 tuşu maliyetsiz ama yine de 1.35 sn pencere+bekleme kilidi yiyor. Bu bilinçli: referans
  seviyenin de bir fırsat maliyeti olsun istedim.

## Bir gün daha olsa

- Profil `speedScale` değerlerini elle değil, başsız koşucuda hedef bitiş süresine göre
  ikili aramayla oturtmak.
- Slipstream: önündeki araca yakınken taban hızda küçük bir kazanç. Paket dinamiğini
  ekonomiye dokunmadan zenginleştirirdi.
- Rakip kararlarının ekranda daha okunabilir olması için ataktan hemen önce kısa bir
  hazırlık işareti.
- Yeniden oynatma: telemetri zaten bir yarışı birebir kurmaya yetecek veriyi taşıyor.
