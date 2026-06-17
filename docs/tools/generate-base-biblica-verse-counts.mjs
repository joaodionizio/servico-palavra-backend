import { readFile, writeFile } from "node:fs/promises";
import { execFile } from "node:child_process";
import { promisify } from "node:util";
import path from "node:path";

const execFileAsync = promisify(execFile);

const ROOT = process.cwd();
const BOOKS_PATH = path.join(ROOT, "docs/examples/base-biblica-livros.v2.json");
const CHAPTERS_PATH = path.join(ROOT, "docs/examples/base-biblica-capitulos.v2.json");
const VERSES_PATH = path.join(ROOT, "docs/examples/base-biblica-versiculos.v2.json");
const REPORT_PATH = path.join(ROOT, "docs/BIBLE_BASE_V2_VERSE_COUNTS_REPORT.md");

const OT_SOURCE = "https://catholic-resources.org/Bible/OT-Statistics-NAB.htm";
const NT_SOURCE = "https://catholic-resources.org/Bible/NT-Statistics-Greek.htm";

const SOURCE_OT = "Catholic Resources / Felix Just, S.J. (NAB)";
const SOURCE_NT = "Catholic Resources / Felix Just, S.J. (Greek NT with NAB notes)";
const sourceDiscrepancies = [];

const otBookMap = new Map([
  ["Genesis", "Gênesis"],
  ["Exodus", "Êxodo"],
  ["Leviticus", "Levítico"],
  ["Numbers", "Números"],
  ["Deuteronomy", "Deuteronômio"],
  ["Joshua", "Josué"],
  ["Judges", "Juízes"],
  ["Ruth", "Rute"],
  ["1 Samuel", "1 Samuel"],
  ["2 Samuel", "2 Samuel"],
  ["1 Kings", "1 Reis"],
  ["2 Kings", "2 Reis"],
  ["1 Chronicles", "1 Crônicas"],
  ["2 Chronicles", "2 Crônicas"],
  ["Ezra", "Esdras"],
  ["Nehemiah", "Neemias"],
  ["Tobit", "Tobias"],
  ["Judith", "Judite"],
  ["Esther", "Ester"],
  ["1 Maccabees", "1 Macabeus"],
  ["2 Maccabees", "2 Macabeus"],
  ["Job", "Jó"],
  ["Psalms", "Salmos"],
  ["Proverbs", "Provérbios"],
  ["Ecclesiastes", "Eclesiastes"],
  ["Song of Songs", "Cântico dos Cânticos"],
  ["Song of Solomon", "Cântico dos Cânticos"],
  ["Wisdom", "Sabedoria"],
  ["Wisdom of Solomon", "Sabedoria"],
  ["Sirach", "Eclesiástico"],
  ["Sirach/Ecclesiasticus", "Eclesiástico"],
  ["Isaiah", "Isaías"],
  ["Jeremiah", "Jeremias"],
  ["Lamentations", "Lamentações"],
  ["Baruch", "Baruc"],
  ["Ezekiel", "Ezequiel"],
  ["Daniel", "Daniel"],
  ["Hosea", "Oseias"],
  ["Joel", "Joel"],
  ["Amos", "Amós"],
  ["Obadiah", "Abdias"],
  ["Jonah", "Jonas"],
  ["Micah", "Miqueias"],
  ["Nahum", "Naum"],
  ["Habakkuk", "Habacuc"],
  ["Zephaniah", "Sofonias"],
  ["Haggai", "Ageu"],
  ["Zechariah", "Zacarias"],
  ["Malachi", "Malaquias"]
]);

const ntBookMap = new Map([
  ["Matt", "Mateus"],
  ["Mark", "Marcos"],
  ["Luke", "Lucas"],
  ["John", "João"],
  ["Acts", "Atos"],
  ["Rom", "Romanos"],
  ["1Cor", "1 Coríntios"],
  ["2Cor", "2 Coríntios"],
  ["Gal", "Gálatas"],
  ["Eph", "Efésios"],
  ["Phil", "Filipenses"],
  ["Col", "Colossenses"],
  ["1Thess", "1 Tessalonicenses"],
  ["2Thess", "2 Tessalonicenses"],
  ["1Tim", "1 Timóteo"],
  ["2Tim", "2 Timóteo"],
  ["Titus", "Tito"],
  ["Phlm", "Filemon"],
  ["Heb", "Hebreus"],
  ["Jas", "Tiago"],
  ["Jam", "Tiago"],
  ["James", "Tiago"],
  ["1Pet", "1 Pedro"],
  ["1Peter", "1 Pedro"],
  ["2Pet", "2 Pedro"],
  ["2Peter", "2 Pedro"],
  ["1John", "1 João"],
  ["2John", "2 João"],
  ["3John", "3 João"],
  ["Jude", "Judas"],
  ["Rev", "Apocalipse"]
]);

const criticalNotes = new Map([
  ["Ester", "NAB/Catholic Resources usa 16 capitulos para incluir acrescimos gregos; revisar mapeamento pastoral antes da importacao."],
  ["Daniel", "NAB/Catholic Resources usa 14 capitulos; Daniel 13 e 14 registram acrescimos deuterocanonicos."],
  ["Joel", "NAB/Catholic Resources usa 4 capitulos; outras tradicoes podem agrupar em 3."],
  ["Baruc", "NAB/Catholic Resources usa 6 capitulos, incluindo a Carta de Jeremias como capitulo 6."],
  ["Ezequiel", "Catholic Resources registra nota de rearranjo textual; a soma dos capitulos extraidos diverge do total publicado do livro."],
  ["Salmos", "NAB/Catholic Resources usa 150 salmos; numeracao de salmos deve ser revisada pastoralmente."],
  ["Malaquias", "NAB/Catholic Resources usa 3 capitulos; algumas tradicoes usam 4."],
  ["Atos:10", "Contagem NAB usada: 49. A tabela principal grega registra 48 e a tabela comparativa marca NAB com 49."],
  ["Apocalipse:12", "Contagem usada: 18. Catholic Resources registra divergencia em outras tradicoes, mas NAB tambem usa 18."],
  ["2 Coríntios:13", "Contagem usada: 13. Catholic Resources registra divergencia em algumas tradicoes com 14."],
  ["3 João:1", "Contagem usada: 15. Catholic Resources registra divergencia em algumas tradicoes com 14."]
]);

function decodeEntities(value) {
  return value
    .replace(/&nbsp;/gi, " ")
    .replace(/&amp;/gi, "&")
    .replace(/&quot;/gi, "\"")
    .replace(/&#39;/gi, "'")
    .replace(/&rsquo;/gi, "'")
    .replace(/&ldquo;|&rdquo;/gi, "\"")
    .replace(/&ndash;|&mdash;/gi, "-");
}

function textFromCell(cell) {
  return decodeEntities(cell)
    .replace(/<script[\s\S]*?<\/script>/gi, "")
    .replace(/<style[\s\S]*?<\/style>/gi, "")
    .replace(/<[^>]+>/g, " ")
    .replace(/\s+/g, " ")
    .trim();
}

function cleanBookName(value) {
  return value.replace(/\*/g, "").replace(/\s+/g, " ").trim();
}

function numeric(value) {
  const match = value.replace(/,/g, "").match(/\d+/);
  return match ? Number.parseInt(match[0], 10) : null;
}

function extractRows(html) {
  return [...html.matchAll(/<tr\b[\s\S]*?<\/tr>/gi)]
    .map((row) => [...row[0].matchAll(/<td\b[\s\S]*?<\/td>/gi)].map((cell) => textFromCell(cell[0])))
    .filter((cells) => cells.length > 0);
}

function parseOt(html) {
  const counts = new Map();
  const rows = extractRows(html);
  const psalms = [];
  let psalmsTotal = null;

  for (const cells of rows) {
    const index = numeric(cells[0] ?? "");
    const chapters = numeric(cells[2] ?? "");
    const total = numeric(cells[3] ?? "");
    const sourceBook = cleanBookName(cells[1] ?? "");
    const livro = otBookMap.get(sourceBook);

    if (sourceBook.startsWith("Psalms")) {
      psalms.push(...cells.slice(4).map(numeric).filter((value) => value !== null).slice(0, 50));
      psalmsTotal = numeric(cells[3] ?? "") ?? psalmsTotal;
      continue;
    }

    if (!index || !livro || !chapters || !total) {
      continue;
    }

    const verses = cells.slice(4, 4 + chapters).map(numeric);
    if (verses.length !== chapters || verses.some((value) => !value)) {
      throw new Error(`Nao foi possivel extrair todos os capitulos de ${sourceBook}.`);
    }

    const sum = verses.reduce((acc, value) => acc + value, 0);
    if (sum !== total) {
      sourceDiscrepancies.push(`${sourceBook}: soma dos capitulos ${sum}, total publicado ${total}`);
    }

    counts.set(livro, { verses, source: SOURCE_OT, sourceUrl: OT_SOURCE });
  }

  if (psalms.length !== 150 || !psalmsTotal) {
    throw new Error(`Nao foi possivel extrair os 150 Salmos; extraidos ${psalms.length}.`);
  }

  const psalmsSum = psalms.reduce((acc, value) => acc + value, 0);
  if (psalmsSum !== psalmsTotal) {
    sourceDiscrepancies.push(`Psalms: soma dos capitulos ${psalmsSum}, total publicado ${psalmsTotal}`);
  }

  counts.set("Salmos", { verses: psalms, source: SOURCE_OT, sourceUrl: OT_SOURCE });

  return counts;
}

function parseNt(html) {
  const counts = new Map();
  const rows = extractRows(html);

  for (const cells of rows) {
    const key = cleanBookName(cells[0] ?? "").replace(/\s+/g, "");
    const livro = ntBookMap.get(key);
    const chapters = numeric(cells[1] ?? "");

    if (!livro || !chapters) {
      continue;
    }

    const verses = cells.slice(2, 2 + chapters).map(numeric);
    if (verses.length !== chapters || verses.some((value) => !value)) {
      throw new Error(`Nao foi possivel extrair todos os capitulos de ${key}.`);
    }

    if (livro === "Atos") {
      verses[9] = 49;
    }

    counts.set(livro, { verses, source: SOURCE_NT, sourceUrl: NT_SOURCE });
  }

  return counts;
}

function getObservation(livro, capitulo) {
  return criticalNotes.get(`${livro}:${capitulo}`) ?? criticalNotes.get(livro) ?? null;
}

function round2(value) {
  return Math.round(value * 100) / 100;
}

function byDescThenName(a, b) {
  return b.quantidadeVersiculos - a.quantidadeVersiculos || a.livro.localeCompare(b.livro, "pt-BR") || a.capitulo - b.capitulo;
}

function makeMarkdownList(rows, formatter) {
  return rows.map((row, index) => `${index + 1}. ${formatter(row)}`).join("\n");
}

async function fetchText(url) {
  const { stdout } = await execFileAsync("curl", ["-L", "-s", url], { maxBuffer: 10 * 1024 * 1024 });
  if (!stdout.trim()) {
    throw new Error(`Falha ao baixar ${url}: resposta vazia.`);
  }

  return stdout;
}

const [books, chapters, otHtml, ntHtml] = await Promise.all([
  readFile(BOOKS_PATH, "utf8").then(JSON.parse),
  readFile(CHAPTERS_PATH, "utf8").then(JSON.parse),
  fetchText(OT_SOURCE),
  fetchText(NT_SOURCE)
]);

const counts = new Map([...parseOt(otHtml), ...parseNt(ntHtml)]);

const verseRecords = chapters.map((chapter) => {
  const bookCounts = counts.get(chapter.livro);
  if (!bookCounts) {
    throw new Error(`Sem contagem encontrada para ${chapter.livro}.`);
  }

  const quantidadeVersiculos = bookCounts.verses[chapter.capitulo - 1];
  if (!quantidadeVersiculos) {
    throw new Error(`Sem contagem encontrada para ${chapter.livro} ${chapter.capitulo}.`);
  }

  return {
    livro: chapter.livro,
    capitulo: chapter.capitulo,
    quantidadeVersiculos,
    fontePrincipal: bookCounts.source,
    fonteUrl: bookCounts.sourceUrl,
    observacao: getObservation(chapter.livro, chapter.capitulo)
  };
});

const versesByBook = new Map();
for (const record of verseRecords) {
  versesByBook.set(record.livro, (versesByBook.get(record.livro) ?? 0) + record.quantidadeVersiculos);
}

const updatedBooks = books.map((book) => {
  const totalVersiculos = versesByBook.get(book.nome);
  if (!totalVersiculos) {
    throw new Error(`Sem total de versiculos para ${book.nome}.`);
  }

  return {
    ...book,
    totalVersiculos,
    mediaVersiculosPorCapitulo: round2(totalVersiculos / book.totalCapitulos)
  };
});

const verseKey = new Map(verseRecords.map((record) => [`${record.livro}:${record.capitulo}`, record]));
const updatedChapters = chapters.map((chapter) => {
  const verse = verseKey.get(`${chapter.livro}:${chapter.capitulo}`);
  if (!verse) {
    throw new Error(`Sem registro de versiculos para ${chapter.livro} ${chapter.capitulo}.`);
  }

  const quantidadeVersiculos = verse.quantidadeVersiculos;
  return {
    ...chapter,
    quantidadeVersiculos,
    pesoLeitura: quantidadeVersiculos,
    tempoEstimadoMinutos: Math.max(2, Math.ceil(quantidadeVersiculos / 4))
  };
});

const totalVerses = verseRecords.reduce((acc, record) => acc + record.quantidadeVersiculos, 0);
const otVerses = updatedBooks.filter((book) => book.testamento === "Antigo").reduce((acc, book) => acc + book.totalVersiculos, 0);
const ntVerses = updatedBooks.filter((book) => book.testamento === "Novo").reduce((acc, book) => acc + book.totalVersiculos, 0);
const longestChapters = [...verseRecords].sort(byDescThenName).slice(0, 10);
const shortestChapters = [...verseRecords]
  .sort((a, b) => a.quantidadeVersiculos - b.quantidadeVersiculos || a.livro.localeCompare(b.livro, "pt-BR") || a.capitulo - b.capitulo)
  .slice(0, 10);
const heaviestBooks = [...updatedBooks].sort((a, b) => b.totalVersiculos - a.totalVersiculos || a.nome.localeCompare(b.nome, "pt-BR")).slice(0, 10);
const manualReview = verseRecords.filter((record) => record.observacao);

const report = `# BaseBiblica V2 - Verse Counts Report

Esta base ainda não deve ser importada no Neon até revisão final.

## Fontes Usadas

- USCCB/NABRE: https://bible.usccb.org/bible
- Vatican/New American Bible: https://www.vatican.va/archive/ENG0839/_INDEX.HTM
- Catholic Resources / Felix Just, S.J. - OT NAB statistics: ${OT_SOURCE}
- Catholic Resources / Felix Just, S.J. - NT statistics and NAB divergence notes: ${NT_SOURCE}
- Biblia Catolica Ave Maria Online do Lirio Catolico: https://www.liriocatolico.com.br/biblia_online/biblia_ave_maria/

## Totais

| Metrica | Esperado | Encontrado |
| --- | ---: | ---: |
| Livros | 73 | ${updatedBooks.length} |
| Antigo Testamento | 46 | ${updatedBooks.filter((book) => book.testamento === "Antigo").length} |
| Novo Testamento | 27 | ${updatedBooks.filter((book) => book.testamento === "Novo").length} |
| Capitulos | 1334 | ${updatedChapters.length} |
| Versiculos | - | ${totalVerses} |
| Versiculos do Antigo Testamento | - | ${otVerses} |
| Versiculos do Novo Testamento | - | ${ntVerses} |

## Top 10 Capitulos Mais Longos

${makeMarkdownList(longestChapters, (row) => `${row.livro} ${row.capitulo}: ${row.quantidadeVersiculos} versiculos`)}

## Top 10 Capitulos Mais Curtos

${makeMarkdownList(shortestChapters, (row) => `${row.livro} ${row.capitulo}: ${row.quantidadeVersiculos} versiculos`)}

## Top 10 Livros Com Maior Total

${makeMarkdownList(heaviestBooks, (row) => `${row.nome}: ${row.totalVersiculos} versiculos`)}

## Pendencias De Validacao Manual

${manualReview.map((row) => `- ${row.livro} ${row.capitulo}: ${row.observacao}`).join("\n")}

## Divergencias E Decisoes

- Divergencias internas nas fontes tecnicas: ${sourceDiscrepancies.length === 0 ? "nenhuma." : sourceDiscrepancies.join("; ")}.
- Ester: mantidos 16 capitulos conforme draft V2 e estatistica NAB; revisar pastoralmente como apresentar os acrescimos gregos.
- Daniel: mantidos 14 capitulos; Daniel 13 e 14 permanecem como capitulos inteiros no draft.
- Joel: mantidos 4 capitulos conforme NAB; outras tradicoes podem agrupar em 3.
- Baruc: mantidos 6 capitulos, incluindo a Carta de Jeremias como capitulo 6.
- Ezequiel: usados os valores por capitulo publicados, mas marcado para revisao manual porque a propria pagina tecnica registra rearranjo textual e a soma por capitulos nao bate com o total publicado.
- Salmos: mantidos 150 capitulos/salmos; revisar numeracao pastoral antes de qualquer material publico.
- Malaquias: mantidos 3 capitulos conforme NAB; algumas tradicoes usam 4.
- Atos 10: usado 49 conforme nota NAB em Catholic Resources; a tabela grega principal registra 48.
- Apocalipse 12: usado 18; Catholic Resources registra divergencias em algumas tradicoes, mas NAB tambem usa 18.
- 2 Corintios 13: usado 13; algumas tradicoes registram 14.
- 3 Joao: usado 15; algumas tradicoes registram 14.

## Regras Aplicadas

- \`pesoLeitura = quantidadeVersiculos\`.
- \`tempoEstimadoMinutos = max(2, ceil(quantidadeVersiculos / 4))\`.
- Capitulos permanecem inteiros nesta fase.
- Capitulos longos devem ser tratados apenas como candidatos a divisao futura.
- O gerador do plano deve balancear pela soma de \`pesoLeitura\`, nao por quantidade bruta de capitulos.
`;

await writeFile(VERSES_PATH, `${JSON.stringify(verseRecords, null, 2)}\n`);
await writeFile(BOOKS_PATH, `${JSON.stringify(updatedBooks, null, 2)}\n`);
await writeFile(CHAPTERS_PATH, `${JSON.stringify(updatedChapters, null, 2)}\n`);
await writeFile(REPORT_PATH, report);

console.log(JSON.stringify({
  livros: updatedBooks.length,
  capitulos: updatedChapters.length,
  versiculos: totalVerses,
  antigoTestamento: otVerses,
  novoTestamento: ntVerses,
  pendenciasValidacaoManual: manualReview.length
}, null, 2));
