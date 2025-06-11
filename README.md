# QuizApp - Aplikacja do tworzenia i rozwiązywania quizów online

## Spis treści
1. [Wprowadzenie](#wprowadzenie)
2. [Instrukcja uruchomienia](#instrukcja-uruchomienia)
3. [Struktura projektu](#struktura-projektu)
4. [System użytkowników](#system-użytkowników)
5. [Funkcjonalności](#funkcjonalności)
6. [Modele danych](#modele-danych)
7. [Technologie](#technologie)

## Wprowadzenie

QuizApp to aplikacja webowa stworzona przy użyciu ASP.NET Core 8 MVC, która umożliwia tworzenie i rozwiązywanie quizów online. Aplikacja oferuje intuicyjny interfejs do zarządzania quizami, pytaniami i odpowiedziami, a także umożliwia śledzenie statystyk rozwiązywania quizów.

Główne funkcjonalności aplikacji:
- Tworzenie quizów z dowolną liczbą pytań jednokrotnego wyboru
- Zarządzanie pytaniami i odpowiedziami
- Rozwiązywanie quizów
- Przeglądanie statystyk rozwiązywania
- System użytkowników oparty na ASP.NET Identity

## Instrukcja uruchomienia

### Wymagania systemowe
- .NET 8.0 SDK lub nowszy
- Dostęp do bazy danych SQLite (tworzona automatycznie przy pierwszym uruchomieniu)

### Kroki uruchomienia

1. Sklonuj repozytorium na swój lokalny komputer:
   ```
   git clone https://github.com/1x1NEBULAR1x1/QuizApp.git
   ```

2. Przejdź do folderu projektu:
   ```
   cd QuizApp
   ```

3. Uruchom aplikację:
   ```
   dotnet run
   ```

4. Podczas pierwszego uruchomienia:
   - Automatycznie zostanie utworzona baza danych SQLite i zastosowane migracje
   - W konsoli pojawi się pytanie o inicjalizację bazy danych testowymi danymi:
     ```
     Do you want to initialize the database with test data? (y/n)
     ```
   - Wpisz 'y', aby załadować przykładowe dane testowe, lub 'n', aby uruchomić aplikację z pustą bazą danych

5. Otwórz przeglądarkę i przejdź pod adres:
   ```
   http://localhost:5000 lub https://localhost:5001
   ```

6. Dane testowe (dostępne po wybraniu opcji inicjalizacji):
   - Login: test1@example.com, Hasło: Pass123$
   - Login: test2@example.com, Hasło: Pass123$
   - Login: teacher@example.com, Hasło: Pass123$

## Struktura projektu

Projekt został zorganizowany według wzorca MVC (Model-View-Controller):

```
QuizApp/
├── Controllers/         # Kontrolery aplikacji
├── Data/                # Kontekst bazy danych i migracje
├── Models/              # Modele danych
├── ViewModels/          # Modele widoków
├── Views/               # Widoki (interfejs użytkownika)
│   ├── Home/
│   ├── Quizzes/
│   ├── Questions/
│   ├── QuizAttempts/
│   ├── Statistics/
│   └── Shared/          # Współdzielone elementy widoków
├── wwwroot/             # Zasoby statyczne (CSS, JS, obrazy)
│   ├── css/
│   ├── js/
│   └── lib/             # Biblioteki zewnętrzne
└── appsettings.json     # Konfiguracja aplikacji
```

## System użytkowników

Aplikacja wykorzystuje ASP.NET Core Identity do zarządzania użytkownikami.

### Funkcje systemu użytkowników:

1. **Rejestracja i logowanie**:
   - Rejestracja nowych użytkowników z adresem e-mail i hasłem
   - Logowanie istniejących użytkowników
   - Automatyczne przypisywanie quizów do użytkownika, który je utworzył

2. **Uprawnienia**:
   - Tworzenie quizów: wymaga zalogowania
   - Edycja i usuwanie quizów: tylko autor quizu
   - Przeglądanie dostępnych quizów: wszyscy użytkownicy
   - Rozwiązywanie quizów: wszyscy zalogowani użytkownicy
   - Przeglądanie statystyk własnych quizów: tylko autor quizu

3. **Konfiguracja hasła**:
   - Minimalna długość: 6 znaków
   - Dla łatwiejszego testowania wyłączone są typowe wymagania dotyczące złożoności hasła
   - Brak wymogu potwierdzenia adresu e-mail

## Funkcjonalności

### Zarządzanie quizami

1. **Tworzenie quizów**:
   - Tytuł i opis quizu
   - Automatyczne przypisanie do zalogowanego użytkownika
   - Możliwość dodawania dowolnej liczby pytań

2. **Zarządzanie pytaniami**:
   - Dodawanie, edycja i usuwanie pytań
   - Definiowanie wartości punktowej pytania (1-100 punktów)
   - Dodawanie minimum 2 opcji odpowiedzi (bez górnego limitu)
   - Oznaczanie jednej odpowiedzi jako poprawnej

3. **Rozwiązywanie quizów**:
   - Przeglądanie dostępnych quizów
   - Rozwiązywanie quizów krok po kroku
   - Wyświetlanie wyników po zakończeniu
   - Zapisywanie wyników i odpowiedzi użytkownika

4. **Statystyki**:
   - Liczba prób rozwiązania quizu
   - Średni wynik w procentach
   - Szczegółowe statystyki dotyczące poszczególnych pytań i odpowiedzi

## Modele danych

### Główne modele:

1. **Quiz**:
   - Id: unikalny identyfikator quizu
   - Title: tytuł quizu (maksymalnie 100 znaków)
   - Description: opis quizu (maksymalnie 500 znaków)
   - CreatedAt: data utworzenia
   - UserId: identyfikator użytkownika (twórcy)
   - Questions: kolekcja pytań
   - Attempts: kolekcja prób rozwiązania

2. **Question**:
   - Id: unikalny identyfikator pytania
   - Text: treść pytania (maksymalnie 500 znaków)
   - Points: liczba punktów za poprawną odpowiedź (1-100)
   - QuizId: identyfikator quizu, do którego należy pytanie
   - Answers: kolekcja odpowiedzi
   - UserAnswers: kolekcja odpowiedzi użytkowników

3. **Answer**:
   - Id: unikalny identyfikator odpowiedzi
   - Text: treść odpowiedzi (maksymalnie 500 znaków)
   - IsCorrect: flaga określająca, czy odpowiedź jest poprawna
   - QuestionId: identyfikator pytania, do którego należy odpowiedź
   - UserAnswers: kolekcja odpowiedzi użytkowników

4. **QuizAttempt**:
   - Id: unikalny identyfikator próby
   - QuizId: identyfikator quizu
   - UserId: identyfikator użytkownika
   - StartedAt: data rozpoczęcia próby
   - CompletedAt: data zakończenia próby
   - Score: uzyskany wynik
   - TotalPossibleScore: maksymalny możliwy wynik
   - UserAnswers: kolekcja odpowiedzi użytkownika

5. **UserAnswer**:
   - Id: unikalny identyfikator odpowiedzi użytkownika
   - QuizAttemptId: identyfikator próby
   - QuestionId: identyfikator pytania
   - AnswerId: identyfikator wybranej odpowiedzi

## Technologie

Projekt został zrealizowany przy użyciu następujących technologii:

1. **Backend**:
   - ASP.NET Core 8 MVC
   - Entity Framework Core
   - ASP.NET Core Identity
   - SQLite

2. **Frontend**:
   - HTML5
   - CSS3
   - Bootstrap 5
   - JavaScript
   - jQuery

3. **Narzędzia**:
   - Visual Studio Code
   - .NET CLI
   - Entity Framework Core Tools 

## Autorzy

- Kyrylo Belichenko 