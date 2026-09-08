# Exit 8 — metro koridoru prototipi

Unity ile geliştirilmiş, The Exit 8'den esinlenen birinci şahıs korku oyunu prototipi. Projedeki uygulama adı şu anda **Exit 7**. Orijinal oyunla bağlantılı değildir.

## Unity'de açma

1. Bu depoyu klonlayın.
2. Unity Hub üzerinden proje klasörünü ekleyin ve **Unity 6000.5.6f1** ile açın.
3. Paketlerin ve varlıkların içe aktarılmasını bekleyin.
4. `Assets/Scenes/Underground.unity` sahnesini açıp Play'e basın.

Post Processing 3.5.4 ve gerekli Unity modülleri `Packages/manifest.json` üzerinden yüklenir. Pişirilmiş ışık haritaları ve kullanılan modeller depodadır. `Library`, yerel günlükler ve Windows derlemesi Git'e dahil edilmez.

## Oynanış

İlk geçiş normal koridordur. Bir anomali görürsen geri dön; yoksa ilerle. Sekiz doğru karar çıkışı açar. Yanlış karar ilerlemeyi sıfırlar. Koridor her turda aynı düzenle tekrarlanır; tabelalar ilerlemeyi gösterir.

- 14 anomali: görsel değişiklikler, kapı vuruntusu, bakış ve koşu sesine tepki veren takipçi, tavanda sürünen yaratık, elektrik kesintisi, su baskını, havalandırmadan süzülen kan ve kapı aralığından uzanan eller.
- Uzaktaki sağ yan koridordan kesintisiz yürüyerek gelen yolcu; avuca hizalanan çanta sapı, parmak tutuşu ve mesafeye bağlı ayak sesleri.
- Oyuncu ilerledikten sonra görünmeyen yan koridordan gelen büyük su dalgası. Suya yakalanmak turu sıfırlar.
- Yaklaşık 45 cm, bir zemin karosu genişliğinde kabartmalı sarı yönlendirme şeridi.
- Yaklaşık 3,8 saniyelik koşu dayanıklılığı; yürürken toparlanma. Takipçiler önce sesle uyarır, yaklaşınca yakalar ve turu sıfırlar.
- İlk normal geçişten sonra kan veya kapı karşılaşması gelir; ilerledikçe tehditlerin sıklığı artar.
- 4K dahili render seçeneği, tam ekran, dönüşte motion blur ve koşma kamera efektleri.

Su efekti hareketli dalga geometrisi, kırılma, köpük ve sıçrama parçacıkları kullanır; tam akışkan simülasyonu değildir. Görsel kalite üzerinde geliştirme devam ediyor.

## Kontroller

| Tuş | İşlev |
| --- | --- |
| WASD / fare | Yürüme / bakış |
| Shift | Dayanıklılık yettiği sürece koşma |
| Ctrl / Space | Eğilme / zıplama |
| Sağ fare, basılı | Hafif yakınlaştırma |
| Esc / sol tıklama | Fareyi serbest bırakma / oyuna dönme |
| R | Yeni oyun |
| F1 | Kontrol yardımı |
| F2 | Görüntü seçenekleri: 4K, 1440p, 1080p |
| F11 | Tam ekran / pencere |

## Tehditleri okuma

- Uzun takipçiye bakarken sakin ilerle veya geri çekil. Bakışını çevirmek ve koşmak yaklaşmasını hızlandırır.
- Tavandaki sürünme ve metal sesi bir tehdittir; koşarak altından geçmeye çalışma.
- Havalandırmadaki koyu kırmızı akış ve kapı aralığındaki eller geri dönme işaretidir.
- Kalp atışı yaklaşan tehlikeyi duyurur. Yakalanınca kısa bir kararma ile başlangıca dönülür.

## Görüntü ayarları

Hierarchy'deki **GÖRÜNTÜ AYARLARI** objesini seçin veya `Exit 7 > Görüntü Ayarlarını Seç` menüsünü kullanın. Inspector'dan ışık, pozlama, yüzey yansımaları, tavan, bloom, AO, grain ve motion blur ayarlanabilir. Kalıcı düzenlemeleri Play kapalıyken yapın. Pişirilmiş ışıkları değiştirdikten sonra Inspector'daki yeniden hesaplama düğmesini kullanın.

## Derleme ve doğrulama

Windows x64 derlemesi için Unity'yi `-batchmode -quit -projectPath <proje-klasörü> -executeMethod BuildStation.BuildPlayer` argümanlarıyla çalıştırın. Çıktı: `Build/Exit7.exe` ve yanındaki veri dosyaları.

Yeni korku sistemi: `Exit 7 > Review > Run horror gameplay checks`. Çıktılar `Temp/HorrorReview/` altında; bakış takibi, yaklaşma, yakalanma, dayanıklılık, anomali temizliği ve malzeme derlemesi kontrol edilir.

Unity içinde hızlı görsel doğrulama: `Exit 7 > Review > Run encounter visual checks`. Sağ köşeden giriş, animasyon boyunca çanta tutuşu, su uyarısı, yaklaşan dalga ve yakalanma sonrası sıfırlama kontrol edilir. Görüntüler ve rapor `Temp/VisualReview/` klasörüne yazılır; sonunda oyun başlangıca döner.

Koridor, anomaliler, su tetiklenmesi ve yolcu hareketi kontrolleri: `-batchmode -projectPath <proje-klasörü> -executeMethod CorridorLoopQA.Run`. Test çalışması kendi tamamlanma ve çıkışını yönetir.

`Assets/Editor` içindeki eski sahne oluşturma araçları geçmiş geliştirme sürümlerine aittir ve mevcut sahneyi değiştirebilir. Güncel kayıtlı sahneyi kullanın; yalnızca derleme almak için `BuildStation.BuildPlayer` yeterlidir.

## Kaynaklar

- Düzenlenebilir Blender dosyaları ve üretim betikleri: `SourceAssets/`.
- Güncel su baskını videosu: [flood-preview.mp4](Screenshots/flood-preview.mp4).
- Model kaynakları ve lisansları: [CREDITS.txt](Assets/ImportedModels/CREDITS.txt).
- Microsoft Rocketbox yolcu modeli ve yürüyüş animasyonu: [MIT lisansı](Assets/Commuter/LICENSE.md).
- Doku ve afiş kaynakları: [ART-SOURCES.md](SourceAssets/ART-SOURCES.md).
- Ayak sesleri: [SOURCE.txt](Assets/Audio/Footsteps/SOURCE.txt).
- Oyun yapısı araştırma notları: [EXIT8-RESEARCH.md](SourceAssets/EXIT8-RESEARCH.md).

Üçüncü taraf varlıkların lisansları kendi dosyalarında belirtilmiştir.
