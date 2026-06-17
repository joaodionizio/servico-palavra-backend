using System.Text.Json;

namespace ServicoPalavra.UnitTests;

public sealed class BaseBiblicaV2JsonTests
{
    private static readonly ExpectedBook[] ExpectedBooks =
    [
        new("1-joao", "1 Jo\u00e3o", 5, "Novo", "Cartas Joaninas", "Cartas Joaninas"),
        new("2-joao", "2 Jo\u00e3o", 1, "Novo", "Cartas Joaninas", "Cartas Joaninas"),
        new("3-joao", "3 Jo\u00e3o", 1, "Novo", "Cartas Joaninas", "Cartas Joaninas"),
        new("joao", "Jo\u00e3o", 21, "Novo", "Evangelhos", "Evangelho Joanino"),
        new("mateus", "Mateus", 28, "Novo", "Evangelhos", "Evangelhos Sinoticos"),
        new("marcos", "Marcos", 16, "Novo", "Evangelhos", "Evangelhos Sinoticos"),
        new("lucas", "Lucas", 24, "Novo", "Evangelhos", "Evangelhos Sinoticos"),
        new("atos", "Atos", 28, "Novo", "Cartas Apostolicas", "Historia da Igreja Primitiva"),
        new("romanos", "Romanos", 16, "Novo", "Cartas Apostolicas", "Cartas Paulinas"),
        new("1-corintios", "1 Cor\u00edntios", 16, "Novo", "Cartas Apostolicas", "Cartas Paulinas"),
        new("2-corintios", "2 Cor\u00edntios", 13, "Novo", "Cartas Apostolicas", "Cartas Paulinas"),
        new("galatas", "G\u00e1latas", 6, "Novo", "Cartas Apostolicas", "Cartas Paulinas"),
        new("efesios", "Ef\u00e9sios", 6, "Novo", "Cartas Apostolicas", "Cartas Paulinas"),
        new("filipenses", "Filipenses", 4, "Novo", "Cartas Apostolicas", "Cartas Paulinas"),
        new("colossenses", "Colossenses", 4, "Novo", "Cartas Apostolicas", "Cartas Paulinas"),
        new("1-tessalonicenses", "1 Tessalonicenses", 5, "Novo", "Cartas Apostolicas", "Cartas Paulinas"),
        new("2-tessalonicenses", "2 Tessalonicenses", 3, "Novo", "Cartas Apostolicas", "Cartas Paulinas"),
        new("1-timoteo", "1 Tim\u00f3teo", 6, "Novo", "Cartas Apostolicas", "Cartas Pastorais"),
        new("2-timoteo", "2 Tim\u00f3teo", 4, "Novo", "Cartas Apostolicas", "Cartas Pastorais"),
        new("tito", "Tito", 3, "Novo", "Cartas Apostolicas", "Cartas Pastorais"),
        new("filemon", "Filemon", 1, "Novo", "Cartas Apostolicas", "Cartas Paulinas"),
        new("hebreus", "Hebreus", 13, "Novo", "Cartas Apostolicas", "Cartas Apostolicas"),
        new("tiago", "Tiago", 5, "Novo", "Cartas Apostolicas", "Cartas Catolicas"),
        new("1-pedro", "1 Pedro", 5, "Novo", "Cartas Apostolicas", "Cartas Catolicas"),
        new("2-pedro", "2 Pedro", 3, "Novo", "Cartas Apostolicas", "Cartas Catolicas"),
        new("judas", "Judas", 1, "Novo", "Cartas Apostolicas", "Cartas Catolicas"),
        new("apocalipse", "Apocalipse", 22, "Novo", "Cartas Apostolicas", "Literatura Apocaliptica"),
        new("genesis", "G\u00eanesis", 50, "Antigo", "Pentateuco", "Tora"),
        new("exodo", "\u00caxodo", 40, "Antigo", "Pentateuco", "Tora"),
        new("levitico", "Lev\u00edtico", 27, "Antigo", "Pentateuco", "Tora"),
        new("numeros", "N\u00fameros", 36, "Antigo", "Pentateuco", "Tora"),
        new("deuteronomio", "Deuteron\u00f4mio", 34, "Antigo", "Pentateuco", "Tora"),
        new("josue", "Josu\u00e9", 24, "Antigo", "Historicos", "Conquista e Juizes"),
        new("juizes", "Ju\u00edzes", 21, "Antigo", "Historicos", "Conquista e Juizes"),
        new("rute", "Rute", 4, "Antigo", "Historicos", "Conquista e Juizes"),
        new("1-samuel", "1 Samuel", 31, "Antigo", "Historicos", "Monarquia"),
        new("2-samuel", "2 Samuel", 24, "Antigo", "Historicos", "Monarquia"),
        new("1-reis", "1 Reis", 22, "Antigo", "Historicos", "Monarquia"),
        new("2-reis", "2 Reis", 25, "Antigo", "Historicos", "Monarquia"),
        new("1-cronicas", "1 Cr\u00f4nicas", 29, "Antigo", "Historicos", "Cronicas e Retorno"),
        new("2-cronicas", "2 Cr\u00f4nicas", 36, "Antigo", "Historicos", "Cronicas e Retorno"),
        new("esdras", "Esdras", 10, "Antigo", "Historicos", "Cronicas e Retorno"),
        new("neemias", "Neemias", 13, "Antigo", "Historicos", "Cronicas e Retorno"),
        new("ester", "Ester", 16, "Antigo", "Historicos", "Narrativas com acrescimos deuterocanonicos"),
        new("isaias", "Isa\u00edas", 66, "Antigo", "Profetas", "Profetas Maiores"),
        new("jeremias", "Jeremias", 52, "Antigo", "Profetas", "Profetas Maiores"),
        new("lamentacoes", "Lamenta\u00e7\u00f5es", 5, "Antigo", "Profetas", "Profetas Maiores"),
        new("ezequiel", "Ezequiel", 48, "Antigo", "Profetas", "Profetas Maiores"),
        new("daniel", "Daniel", 14, "Antigo", "Profetas", "Profetas Maiores com acrescimos deuterocanonicos"),
        new("oseias", "Oseias", 14, "Antigo", "Profetas", "Profetas Menores"),
        new("joel", "Joel", 4, "Antigo", "Profetas", "Profetas Menores"),
        new("amos", "Am\u00f3s", 9, "Antigo", "Profetas", "Profetas Menores"),
        new("abdias", "Abdias", 1, "Antigo", "Profetas", "Profetas Menores"),
        new("jonas", "Jonas", 4, "Antigo", "Profetas", "Profetas Menores"),
        new("miqueias", "Miqueias", 7, "Antigo", "Profetas", "Profetas Menores"),
        new("naum", "Naum", 3, "Antigo", "Profetas", "Profetas Menores"),
        new("habacuc", "Habacuc", 3, "Antigo", "Profetas", "Profetas Menores"),
        new("sofonias", "Sofonias", 3, "Antigo", "Profetas", "Profetas Menores"),
        new("ageu", "Ageu", 2, "Antigo", "Profetas", "Profetas Menores"),
        new("zacarias", "Zacarias", 14, "Antigo", "Profetas", "Profetas Menores"),
        new("malaquias", "Malaquias", 3, "Antigo", "Profetas", "Profetas Menores"),
        new("jo", "J\u00f3", 42, "Antigo", "Sapienciais", "Sapienciais"),
        new("salmos", "Salmos", 150, "Antigo", "Sapienciais", "Poeticos"),
        new("proverbios", "Prov\u00e9rbios", 31, "Antigo", "Sapienciais", "Sapienciais"),
        new("eclesiastes", "Eclesiastes", 12, "Antigo", "Sapienciais", "Sapienciais"),
        new("cantico-dos-canticos", "C\u00e2ntico dos C\u00e2nticos", 8, "Antigo", "Sapienciais", "Poeticos"),
        new("tobias", "Tobias", 14, "Antigo", "Deuterocanonicos", "Deuterocanonico Historico"),
        new("judite", "Judite", 16, "Antigo", "Deuterocanonicos", "Deuterocanonico Historico"),
        new("sabedoria", "Sabedoria", 19, "Antigo", "Deuterocanonicos", "Deuterocanonico Sapiencial"),
        new("eclesiastico", "Eclesi\u00e1stico", 51, "Antigo", "Deuterocanonicos", "Deuterocanonico Sapiencial"),
        new("baruc", "Baruc", 6, "Antigo", "Deuterocanonicos", "Deuterocanonico Profetico"),
        new("1-macabeus", "1 Macabeus", 16, "Antigo", "Deuterocanonicos", "Deuterocanonico Historico"),
        new("2-macabeus", "2 Macabeus", 15, "Antigo", "Deuterocanonicos", "Deuterocanonico Historico")
    ];

    [Fact]
    public void BaseBiblicaV2Json_matches_catholic_canon_and_pastoral_order()
    {
        var root = FindRepositoryRoot();
        var books = LoadJsonArray(root, "docs/examples/base-biblica-livros.v2.json");
        var chapters = LoadJsonArray(root, "docs/examples/base-biblica-capitulos.v2.json");
        var verseCounts = LoadJsonArray(root, "docs/examples/base-biblica-versiculos.v2.json");
        var failures = new List<string>();

        ValidateBookList(books, failures);
        ValidateChapterList(books, chapters, failures);
        ValidateVerseCounts(books, chapters, verseCounts, failures);
        ValidateNoForbiddenTerms(root, failures);

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private static void ValidateBookList(IReadOnlyList<JsonElement> books, List<string> failures)
    {
        failures.AddIf(books.Count != 73, $"Total de livros esperado 73, encontrado {books.Count}.");
        failures.AddIf(books.Count(x => Text(x, "testamento") == "Antigo") != 46, "Total de livros do Antigo Testamento divergente.");
        failures.AddIf(books.Count(x => Text(x, "testamento") == "Novo") != 27, "Total de livros do Novo Testamento divergente.");

        var expectedSlugs = ExpectedBooks.Select(x => x.Slug).ToHashSet();
        var actualSlugs = books.Select(x => Text(x, "slug")).ToList();
        var duplicateSlugs = actualSlugs.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
        failures.AddIf(duplicateSlugs.Length > 0, $"Slugs duplicados: {string.Join(", ", duplicateSlugs)}.");

        var missingSlugs = expectedSlugs.Except(actualSlugs).ToArray();
        var extraSlugs = actualSlugs.Except(expectedSlugs).ToArray();
        failures.AddIf(missingSlugs.Length > 0, $"Livros faltando: {string.Join(", ", missingSlugs)}.");
        failures.AddIf(extraSlugs.Length > 0, $"Livros extras: {string.Join(", ", extraSlugs)}.");

        var duplicateNames = books.Select(x => Text(x, "nome")).GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
        failures.AddIf(duplicateNames.Length > 0, $"Livros duplicados por nome: {string.Join(", ", duplicateNames)}.");

        var bySlug = books.ToDictionary(x => Text(x, "slug"));
        for (var i = 0; i < ExpectedBooks.Length; i++)
        {
            var expected = ExpectedBooks[i];
            if (!bySlug.TryGetValue(expected.Slug, out var actual))
            {
                continue;
            }

            failures.AddIf(Text(actual, "nome") != expected.Name, $"{expected.Slug}: nome esperado '{expected.Name}', encontrado '{Text(actual, "nome")}'.");
            failures.AddIf(Int(actual, "totalCapitulos") != expected.Chapters, $"{expected.Name}: totalCapitulos esperado {expected.Chapters}, encontrado {Int(actual, "totalCapitulos")}.");
            failures.AddIf(Text(actual, "testamento") != expected.Testament, $"{expected.Name}: testamento inconsistente.");
            failures.AddIf(Text(actual, "grupoPastoral") != expected.Group, $"{expected.Name}: grupoPastoral inconsistente.");
            failures.AddIf(Text(actual, "subgrupo") != expected.Subgroup, $"{expected.Name}: subgrupo inconsistente.");
            failures.AddIf(Int(actual, "ordemLivro") != i + 1, $"{expected.Name}: ordemLivro esperada {i + 1}, encontrada {Int(actual, "ordemLivro")}.");
            failures.AddIf(!Bool(actual, "ativo"), $"{expected.Name}: ativo deve ser true.");
            failures.AddIf(Int(actual, "totalVersiculos") <= 0, $"{expected.Name}: totalVersiculos deve ser maior que zero.");
            failures.AddIf(Decimal(actual, "mediaVersiculosPorCapitulo") <= 0, $"{expected.Name}: mediaVersiculosPorCapitulo deve ser maior que zero.");
        }
    }

    private static void ValidateChapterList(IReadOnlyList<JsonElement> books, IReadOnlyList<JsonElement> chapters, List<string> failures)
    {
        failures.AddIf(chapters.Count != 1334, $"Total de capitulos esperado 1334, encontrado {chapters.Count}.");

        var orders = chapters.Select(x => Int(x, "ordem")).ToArray();
        var duplicateOrders = orders.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
        failures.AddIf(duplicateOrders.Length > 0, $"Ordens duplicadas: {string.Join(", ", duplicateOrders)}.");
        var missingOrders = Enumerable.Range(1, chapters.Count).Except(orders).ToArray();
        failures.AddIf(missingOrders.Length > 0, $"Buracos na ordem: {string.Join(", ", missingOrders.Take(20))}.");
        failures.AddIf(orders.Length > 0 && orders.Max() != chapters.Count, $"Ultima ordem esperada {chapters.Count}, encontrada {orders.Max()}.");

        var booksByName = books.ToDictionary(x => Text(x, "nome"));
        var chaptersByBook = chapters.GroupBy(x => Text(x, "livro")).ToDictionary(x => x.Key, x => x.ToArray());
        var chapterBookNames = chaptersByBook.Keys.ToHashSet();
        var expectedBookNames = ExpectedBooks.Select(x => x.Name).ToHashSet();
        failures.AddIf(expectedBookNames.Except(chapterBookNames).Any(), $"Livros sem capitulos: {string.Join(", ", expectedBookNames.Except(chapterBookNames))}.");
        failures.AddIf(chapterBookNames.Except(expectedBookNames).Any(), $"Capitulos com livro inexistente: {string.Join(", ", chapterBookNames.Except(expectedBookNames))}.");

        var expectedExpanded = new List<(string Book, int Chapter, ExpectedBook Expected)>();
        foreach (var expected in ExpectedBooks)
        {
            if (!chaptersByBook.TryGetValue(expected.Name, out var actualChapters))
            {
                continue;
            }

            var actualNumbers = actualChapters.Select(x => Int(x, "capitulo")).ToArray();
            var expectedNumbers = Enumerable.Range(1, expected.Chapters).ToArray();
            var duplicateNumbers = actualNumbers.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
            var missingNumbers = expectedNumbers.Except(actualNumbers).ToArray();
            var extraNumbers = actualNumbers.Except(expectedNumbers).ToArray();

            failures.AddIf(actualChapters.Length != expected.Chapters, $"{expected.Name}: capitulos esperados {expected.Chapters}, encontrados {actualChapters.Length}.");
            failures.AddIf(duplicateNumbers.Length > 0, $"{expected.Name}: capitulos duplicados {string.Join(", ", duplicateNumbers)}.");
            failures.AddIf(missingNumbers.Length > 0, $"{expected.Name}: capitulos faltando {string.Join(", ", missingNumbers)}.");
            failures.AddIf(extraNumbers.Length > 0, $"{expected.Name}: capitulos extras {string.Join(", ", extraNumbers)}.");

            foreach (var chapter in actualChapters)
            {
                failures.AddIf(!booksByName.ContainsKey(Text(chapter, "livro")), $"{Text(chapter, "livro")}: capitulo sem livro correspondente.");
                failures.AddIf(Text(chapter, "testamento") != expected.Testament, $"{expected.Name} {Int(chapter, "capitulo")}: testamento inconsistente.");
                failures.AddIf(Text(chapter, "grupo") != expected.Group, $"{expected.Name} {Int(chapter, "capitulo")}: grupo inconsistente.");
                failures.AddIf(Text(chapter, "subgrupo") != expected.Subgroup, $"{expected.Name} {Int(chapter, "capitulo")}: subgrupo inconsistente.");
                failures.AddIf(Int(chapter, "tempoEstimadoMinutos") <= 0, $"{expected.Name} {Int(chapter, "capitulo")}: tempoEstimadoMinutos deve ser maior que zero.");
                failures.AddIf(Int(chapter, "quantidadeVersiculos") <= 0, $"{expected.Name} {Int(chapter, "capitulo")}: quantidadeVersiculos deve ser maior que zero.");
                failures.AddIf(Int(chapter, "pesoLeitura") <= 0, $"{expected.Name} {Int(chapter, "capitulo")}: pesoLeitura deve ser maior que zero.");
                failures.AddIf(!Bool(chapter, "ativo"), $"{expected.Name} {Int(chapter, "capitulo")}: ativo deve ser true.");
            }

            expectedExpanded.AddRange(expectedNumbers.Select(chapter => (expected.Name, chapter, expected)));
        }

        var byOrder = chapters.OrderBy(x => Int(x, "ordem")).ToArray();
        for (var i = 0; i < Math.Min(byOrder.Length, expectedExpanded.Count); i++)
        {
            var actual = byOrder[i];
            var expected = expectedExpanded[i];
            failures.AddIf(Text(actual, "livro") != expected.Book || Int(actual, "capitulo") != expected.Chapter,
                $"Ordem pastoral quebrada na posicao {i + 1}: esperado {expected.Book} {expected.Chapter}, encontrado {Text(actual, "livro")} {Int(actual, "capitulo")}.");
        }
    }

    private static void ValidateVerseCounts(
        IReadOnlyList<JsonElement> books,
        IReadOnlyList<JsonElement> chapters,
        IReadOnlyList<JsonElement> verseCounts,
        List<string> failures)
    {
        failures.AddIf(verseCounts.Count != 1334, $"Total de registros de versiculos esperado 1334, encontrado {verseCounts.Count}.");
        failures.AddIf(verseCounts.Sum(x => Int(x, "quantidadeVersiculos")) != 35527, "Total geral de versiculos divergente.");

        var chapterKeys = chapters.Select(x => Key(Text(x, "livro"), Int(x, "capitulo"))).ToHashSet();
        var verseKeys = verseCounts.Select(x => Key(Text(x, "livro"), Int(x, "capitulo"))).ToArray();
        var duplicateVerseKeys = verseKeys.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
        failures.AddIf(duplicateVerseKeys.Length > 0, $"Registros de versiculos duplicados: {string.Join(", ", duplicateVerseKeys)}.");

        var missingVerseKeys = chapterKeys.Except(verseKeys).ToArray();
        var extraVerseKeys = verseKeys.Except(chapterKeys).ToArray();
        failures.AddIf(missingVerseKeys.Length > 0, $"Registros de versiculos faltando: {string.Join(", ", missingVerseKeys.Take(20))}.");
        failures.AddIf(extraVerseKeys.Length > 0, $"Registros de versiculos extras: {string.Join(", ", extraVerseKeys.Take(20))}.");

        var versesByKey = verseCounts.ToDictionary(x => Key(Text(x, "livro"), Int(x, "capitulo")));
        foreach (var chapter in chapters)
        {
            var key = Key(Text(chapter, "livro"), Int(chapter, "capitulo"));
            if (!versesByKey.TryGetValue(key, out var verse))
            {
                continue;
            }

            var quantidadeVersiculos = Int(verse, "quantidadeVersiculos");
            failures.AddIf(quantidadeVersiculos <= 0, $"{key}: quantidadeVersiculos deve ser maior que zero.");
            failures.AddIf(Text(verse, "fontePrincipal").Length == 0, $"{key}: fontePrincipal obrigatoria.");
            failures.AddIf(Text(verse, "fonteUrl").Length == 0, $"{key}: fonteUrl obrigatoria.");
            failures.AddIf(Int(chapter, "quantidadeVersiculos") != quantidadeVersiculos, $"{key}: quantidadeVersiculos divergente entre arquivos.");
            failures.AddIf(Int(chapter, "pesoLeitura") != quantidadeVersiculos, $"{key}: pesoLeitura deve ser igual a quantidadeVersiculos.");
            failures.AddIf(Int(chapter, "tempoEstimadoMinutos") != Math.Max(2, (int)Math.Ceiling(quantidadeVersiculos / 4m)), $"{key}: tempoEstimadoMinutos divergente.");
        }

        var verseTotalsByBook = verseCounts
            .GroupBy(x => Text(x, "livro"))
            .ToDictionary(x => x.Key, x => x.Sum(y => Int(y, "quantidadeVersiculos")));

        foreach (var book in books)
        {
            var bookName = Text(book, "nome");
            if (!verseTotalsByBook.TryGetValue(bookName, out var verseTotal))
            {
                failures.Add($"{bookName}: sem total de versiculos.");
                continue;
            }

            var expectedAverage = Round2(verseTotal / (decimal)Int(book, "totalCapitulos"));
            failures.AddIf(Int(book, "totalVersiculos") != verseTotal, $"{bookName}: totalVersiculos divergente.");
            failures.AddIf(Decimal(book, "mediaVersiculosPorCapitulo") != expectedAverage, $"{bookName}: mediaVersiculosPorCapitulo divergente.");
        }
    }

    private static void ValidateNoForbiddenTerms(PathString root, List<string> failures)
    {
        var files = new[]
        {
            "docs/examples/base-biblica-livros.v2.json",
            "docs/examples/base-biblica-capitulos.v2.json",
            "docs/examples/base-biblica-versiculos.v2.json",
            "docs/examples/base-biblica-capitulos-com-versiculos.v2.json",
            "docs/examples/base-biblica-livros-com-versiculos.v2.json"
        };
        var forbidden = new[]
        {
            "SUP" + "ABASE_URL",
            "SUP" + "ABASE_SERVICE_ROLE_KEY",
            "sup" + "abase",
            "postgres" + "ql://",
            "Pass" + "word=",
            "se" + "nha",
            "to" + "ken",
            "J" + "WT",
            "Bear" + "er",
            "local" + "Storage",
            "session" + "Storage",
            "npg" + "_",
            "se" + "cret"
        };

        foreach (var file in files)
        {
            var text = File.ReadAllText(Path.Combine(root.Value, file));
            var found = forbidden.Where(term => text.Contains(term, StringComparison.OrdinalIgnoreCase)).ToArray();
            failures.AddIf(found.Length > 0, $"{file}: termos proibidos encontrados: {string.Join(", ", found)}.");
        }

        var jsonFiles = files.Where(x => x.EndsWith(".json", StringComparison.Ordinal)).ToArray();
        var forbiddenJsonProperties = new[] { "texto", "conteudo", "conteúdo", "leituraTexto", "versiculoTexto" };
        foreach (var file in jsonFiles)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root.Value, file)));
            var propertyNames = document.RootElement
                .EnumerateArray()
                .SelectMany(x => x.EnumerateObject().Select(y => y.Name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var found = forbiddenJsonProperties.Where(term => propertyNames.Contains(term, StringComparer.OrdinalIgnoreCase)).ToArray();
            failures.AddIf(found.Length > 0, $"{file}: propriedades de texto biblico encontradas: {string.Join(", ", found)}.");
        }
    }

    private static IReadOnlyList<JsonElement> LoadJsonArray(PathString root, string relativePath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root.Value, relativePath)));
        return document.RootElement.EnumerateArray().Select(x => x.Clone()).ToArray();
    }

    private static PathString FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "docs", "examples")))
            {
                return new PathString(directory.FullName);
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Nao foi possivel localizar a raiz do repositorio com docs/examples.");
    }

    private static string Text(JsonElement element, string property) => element.GetProperty(property).GetString() ?? string.Empty;
    private static int Int(JsonElement element, string property) => element.GetProperty(property).GetInt32();
    private static decimal Decimal(JsonElement element, string property) => element.GetProperty(property).GetDecimal();
    private static bool Bool(JsonElement element, string property) => element.GetProperty(property).GetBoolean();
    private static string Key(string book, int chapter) => $"{book}:{chapter}";
    private static decimal Round2(decimal value) => Math.Round(value * 100m, MidpointRounding.AwayFromZero) / 100m;

    private sealed record ExpectedBook(string Slug, string Name, int Chapters, string Testament, string Group, string Subgroup);
    private sealed record PathString(string Value);
}

file static class BaseBiblicaV2TestExtensions
{
    public static void AddIf(this List<string> failures, bool condition, string message)
    {
        if (condition)
        {
            failures.Add(message);
        }
    }
}
