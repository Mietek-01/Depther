# Depther
![Browser Screenshot](https://github.com/Mietek-01/Depther/blob/master/Screens/Start%20Menu.png)

Jest to moja autorska gra, którą zacząłem tworzyć w lutym 2021 roku z wykorzystaniem silnika Unity, a moim głównym celem było nabycie umiejętności z zakresu tworzenia gier. 

Jestem pasjonatem programowania, zatem głównie skupiłem się na mechanice gry, niż na jej oprawie wizualnej. Bardzo zależało mi na różnorodności oraz złożoności gry, by była wyjątkowa i cechowała się bogatą mechaniką.  

Niestety grafikę oraz audio musiałem pobrać z innych projektów, jednak były to projekty publiczne przeznaczone do nauki. Grafikę jedynie musiałem trochę przerobić, by pasowała do klimatu gry. 

## Opis gry
Depther jest platformówką, ale nie byle jaką. W początkowej fazie tworzenia gry bardzo spodobał mi się efekt głębi,
który można osiągnąć w bardzo prosty sposób poprzez manipulację pozycją „Z” obiektów.

Zdecydowałem, że stworze grę 2D, ale taką która w dynamiczny sposób wykorzystuje trzeci wymiar, co okazało się nie lada wyzwaniem. 

![Browser Screenshot](https://github.com/Mietek-01/Depther/blob/master/Screens/Z3.1.png)

![Browser Screenshot](https://github.com/Mietek-01/Depther/blob/master/Screens/Z1.2.png)

![Browser Screenshot](https://github.com/Mietek-01/Depther/blob/master/Screens/Z0.2.png)

![Browser Screenshot](https://github.com/Mietek-01/Depther/blob/master/Screens/Z2.1.png)

![Browser Screenshot](https://github.com/Mietek-01/Depther/blob/master/Screens/Z4.1.png)

![Browser Screenshot](https://github.com/Mietek-01/Depther/blob/master/Screens/Z5.2.png)

![Browser Screenshot](https://github.com/Mietek-01/Depther/blob/master/Screens/Z1.1.png)

Myślę, że dzięki temu udało mi się osiągnąć naprawdę fajny efekt, który wyróżnia tę platformówkę na tle innych tj. "Super Mario Bros", którego remake-a 
swoją drogą stworzyłem bedąc w drugiej liceum 
[ i znajduje się w tym repozytorium.](https://github.com/Mietek-01/Super-Mario-Bros-Remake)

Dodatkowo gra zawiera samouczek oraz sprawny system checkpoint-ów dzięki, którym w szybki sposób gracz będzie mógł się odrodzić nie tracąc postępów w rozgrywce. 

![Browser Screenshot](https://github.com/Mietek-01/Depther/blob/master/Screens/Z0.1.png)

W celu łatwiejszej eksploracji świata gry na końcu sekcji "About" w Start Menu umieściłem spis cheat-ów dzięki, którym gracz np. nie będzie mógł przyjmować obrażeń.

## Odnośniki
- Razem z kolegą stworzyliśmy amatorski trailer i znajduje się na youtube [o tutaj](https://youtu.be/bA7vMUqEdhA).
- Jeśli chcesz zagrać możesz wejśc w [ten link](https://mietek01.itch.io/depther), który prowadzić do strony itch.io, specjalnej platformy do umieszczania gier. By zagrać wpisz hasło: Depther714. Z powodu, iż gra będzie uruchamiana na przeglądarce, FPS mogą być niższe niż w wersji na PC. 
- Możesz również pobrać folder Depther.zip z sekcji [Releases](https://github.com/Mietek-01/Depther/releases). Gra jest zrobiona na silniku, więc nie masz się czego obawiać 😉
- W [VSCodeCounter](https://github.com/Mietek-01/Depther/blob/master/.VSCodeCounter/2022-03-08_13-50-04/results.md) znajduje się spis wszystkich skryptów gry.

## Opis projektu
Projekt gry zawiera ponad 90 skryptów (11 000 linijek kodu), które własnoręcznie napisałem. Przez cały proces tworzenia "Depthera" dużą wagę przywiązywałem do refactoringu, gdyż jako programiście bardzo zależy mi na jakości, jak również czystości kodu. 

Większość mechanizmów, które w grze się znajdują musiałem wielokrotnie przebudowywać, gdyż mój kod nie nadążał za wzrostem mojej wiedzy jak i doświadczenia. Zdaję sobie sprawę, że i teraz wiele rzeczy napisał bym inaczej zwłaszcza, że o wzorcach projektowych, zasadach SOLID, Unit Test-ach oraz wzorcu DI zaczołem uczyć się, gdy moja gra była już praktycznie skończona.

Poniżej zamieszczam spis najciekawszych elementów gry, które w moim mniemaniu są najbardziej warte pokazania. Wszystkie przedstawiane skrypty znajdują się w folderze [Highlighted Scripts](https://github.com/Mietek-01/Depther/tree/master/Highlighted%20Scripts).

### Abstrakcje
- W folderze [Abstractions](https://github.com/Mietek-01/Depther/tree/master/Highlighted%20Scripts/Abstractions) znajdują się moje klasy abstrakcyjne. Dzięki nim kod działa w większym stopniu ogólności, co przekłada się na jego uniwersalność jak również rozszerzalność. 
- Szczególnie istotna jest klasa [Damageable](https://github.com/Mietek-01/Depther/blob/master/Highlighted%20Scripts/Abstractions/Damageable.cs), dzięki której instancje klas pochodnych stają się zniszczalne i mogą przyjmować obrażenia. Co więcej, dzięki zastosowaniu wzorca "Observer", w momencie gdy taki obiekt zostanie zniszczony, wszyscy subskrybenci zostaną o tym poinformowani, co pozwala obiektom śledzącym zareagować adekwatnie do zaistniałego zdarzenia.

### Mechanizm Player-a
- Główna część znajduję się w klasie [Player](https://github.com/Mietek-01/Depther/blob/master/Highlighted%20Scripts/Player/Player.cs), która dziedziczy z klasy [Character](https://github.com/Mietek-01/Depther/blob/master/Highlighted%20Scripts/Abstractions/Character.cs). W niej zastosowałem wzorzec DI, poprzez użycie interfejsu IPlayerInputData.
- Za pobieranie input-a odpowiada skrypt [PlayerInput.cs](https://github.com/Mietek-01/Depther/blob/master/Highlighted%20Scripts/Player/PlayerInput.cs).
- W folderze [InputData](https://github.com/Mietek-01/Depther/tree/master/Highlighted%20Scripts/Player/InputData) znajdują się klasy, które umożliwiają mi pobieranie inputa-a od gracza. Bardzo ważną rolę odgrywa klasa [InputData](https://github.com/Mietek-01/Depther/blob/master/Highlighted%20Scripts/Player/InputData/InputData.cs), która jest typem polimorficznym oraz klasą bazową dla klas pochodnych tj. [DoubleTapInput](https://github.com/Mietek-01/Depther/blob/master/Highlighted%20Scripts/Player/InputData/PlayerInput.DoubleTapInput.cs) czy [ButtonInput](https://github.com/Mietek-01/Depther/blob/master/Highlighted%20Scripts/Player/InputData/PlayerInput.ButtonInput.cs). Bardzo fajnie widoczna jest tu zasada "Open/Close" oraz "Liskov substitution".
- Za wykonanie efektu "Dash move" odpowiada skrypt [PlayerDashMoveCreator.cs](https://github.com/Mietek-01/Depther/blob/master/Highlighted%20Scripts/Player/PlayerDashMoveCreator.cs).
- Skrypt [Weapon.cs](https://github.com/Mietek-01/Depther/blob/master/Highlighted%20Scripts/Player/Weapon/Weapon.cs) pozwala nadać broni mocy sprawczej.

### ObjectsPooler
- Skrypt [ObjectsPooler.cs](https://github.com/Mietek-01/Depther/blob/master/Highlighted%20Scripts/ObjectsPooler/ObjectsPooler.cs) jest moją implementacja techniki "Objects Pooling", która pozwala na wtórne używanie obiektów. Stworzyłem ją w oparciu o wzorzec "Singleton", który gwarantuję występowanie tylko jednej instancji klasy oraz zapewnia do niej globalny dostęp. Dodatkowo w celu sprawniejszego pobierania obiektów z "basenu" zastosowałem strukturę danych kolejkę.

### DepthField
- Jest to mechanizm, który znajduje się w folderze [DepthField](https://github.com/Mietek-01/Depther/tree/master/Highlighted%20Scripts/DepthField). Jego zadaniem jest "ściągnięcie" obiektu do określonej pozycji "Z", gdy ten tylko wejdzie w jego pole. Pole to składa się z zewnętrznego oraz wewnętrznego collidera w kształcie koła. Algorytm na podstawie odległości danego obiektu od środka tego pola ustawia odpowiednią pozycję "Z" obiektu.

### Finding Tangent Algorithm
- Jest to algorytm, który znajduję się w skrypcie [UsefulFunctions.cs](https://github.com/Mietek-01/Depther/blob/master/Highlighted%20Scripts/UsefulFunctions/UsefulFunctions.cs), a dokładniej w funkcji UsefulFunctions.FindTangentFor(...). Zadaniem tego algorytmu jest określenie stycznej do collidera, z którym obiekt złapał kolizje. Owy algorytm był mi potrzebny, by w odpowiedni sposób tworzyć efekty rosprysku, w momencie gdy pocisk uderza w platforme pod różnymi kątami. Dzięki zmiennej UsefulFunctions.TestMode można zobaczyć jak algorytm działa w czasie rzeczywistym.

### Unit Tests
- Moje testy jednostkowe znajdują się w folderze [Tests](https://github.com/Mietek-01/Depther/tree/master/Highlighted%20Scripts/Tests). Nie jest ich za wiele, gdyż nie miałem na to zbyt dużo czasu oraz ich naukę zacząłem gdy "Depther" był już praktycznie skończony, także struktura kodu nie pozwoliła mi na ich łatwą implementację. Jestem przekonany, że gdybym zaczą od początku tworzyc swoją grę z uwzględnieniem Unit Test-ów, to struktura kodu mogłaby wyglądać zupełnie inaczej.   

## Widok projektu

![Browser Screenshot](https://github.com/Mietek-01/Depther/blob/master/Screens/Unity%20Project%20Screen.png)

## Podsumowanie
Na koniec chciałbym dodać, że jestem całkowitym samoukiem, który musiał zmierzyć się ze wszystkim sam oraz któremu to właśnie pasja pozwoliła stworzyć tak rozbudowaną grę samodzielnie.
