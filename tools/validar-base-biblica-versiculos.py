#!/usr/bin/env python3
import json
from pathlib import Path
from collections import Counter, defaultdict

BASE = Path(__file__).resolve().parent
ROOT = BASE.parent

books_path = ROOT / "docs/examples/base-biblica-livros-com-versiculos.v2.json"
chapters_path = ROOT / "docs/examples/base-biblica-capitulos-com-versiculos.v2.json"
verses_path = ROOT / "docs/examples/base-biblica-versiculos.v2.json"

books = json.loads(books_path.read_text(encoding="utf-8"))
chapters = json.loads(chapters_path.read_text(encoding="utf-8"))
verses = json.loads(verses_path.read_text(encoding="utf-8"))

errors = []

def fail(msg):
    errors.append(msg)

if len(books) != 73:
    fail(f"Total de livros inválido: {len(books)} != 73")
if sum(1 for b in books if b["testamento"] == "Antigo") != 46:
    fail("Total de livros do Antigo Testamento inválido.")
if sum(1 for b in books if b["testamento"] == "Novo") != 27:
    fail("Total de livros do Novo Testamento inválido.")
if len(chapters) != 1334:
    fail(f"Total de capítulos inválido: {len(chapters)} != 1334")
if len(verses) != 1334:
    fail(f"Total de registros de versículos inválido: {len(verses)} != 1334")

slug_counts = Counter(b["slug"] for b in books)
for slug, count in slug_counts.items():
    if count != 1:
        fail(f"Slug duplicado: {slug}")

book_by_name = {b["nome"]: b for b in books}
for livro, count in Counter(b["nome"] for b in books).items():
    if count != 1:
        fail(f"Livro duplicado: {livro}")

orders = [c["ordem"] for c in chapters]
if orders != list(range(1, len(chapters) + 1)):
    fail("Campo ordem dos capítulos não é sequencial começando em 1.")

chapters_by_book = defaultdict(list)
for c in chapters:
    chapters_by_book[c["livro"]].append(c)

verses_by_book = defaultdict(list)
for v in verses:
    verses_by_book[v["livro"]].append(v)

for book in books:
    nome = book["nome"]
    expected_total = book["totalCapitulos"]

    if nome not in chapters_by_book:
        fail(f"Livro sem capítulos no arquivo de capítulos: {nome}")
        continue
    if nome not in verses_by_book:
        fail(f"Livro sem registros de versículos: {nome}")
        continue

    chapter_numbers = sorted(c["capitulo"] for c in chapters_by_book[nome])
    expected_numbers = list(range(1, expected_total + 1))
    if chapter_numbers != expected_numbers:
        fail(f"Capítulos inválidos para {nome}: esperado {expected_numbers}, encontrado {chapter_numbers}")

    verse_numbers = sorted(v["capitulo"] for v in verses_by_book[nome])
    if verse_numbers != expected_numbers:
        fail(f"Registros de versículos inválidos para {nome}: esperado {expected_numbers}, encontrado {verse_numbers}")

    chapter_sum = sum(c["quantidadeVersiculos"] for c in chapters_by_book[nome])
    verse_sum = sum(v["quantidadeVersiculos"] for v in verses_by_book[nome])
    if chapter_sum != verse_sum:
        fail(f"Soma capítulo/versículo divergente em {nome}: {chapter_sum} != {verse_sum}")
    if chapter_sum != book["totalVersiculos"]:
        fail(f"totalVersiculos divergente em {nome}: {chapter_sum} != {book['totalVersiculos']}")

    for c in chapters_by_book[nome]:
        if c["quantidadeVersiculos"] <= 0:
            fail(f"quantidadeVersiculos <= 0 em {nome} {c['capitulo']}")
        if c["pesoLeitura"] <= 0:
            fail(f"pesoLeitura <= 0 em {nome} {c['capitulo']}")
        if c["tempoEstimadoMinutos"] <= 0:
            fail(f"tempoEstimadoMinutos <= 0 em {nome} {c['capitulo']}")
        if c["testamento"] != book["testamento"]:
            fail(f"Testamento divergente em {nome} {c['capitulo']}")
        if c["grupo"] != book["grupoPastoral"]:
            fail(f"Grupo divergente em {nome} {c['capitulo']}")
        if c["subgrupo"] != book["subgrupo"]:
            fail(f"Subgrupo divergente em {nome} {c['capitulo']}")

for c in chapters:
    forbidden_tokens = ["postgres" + "ql://", "pass" + "word=", "sup" + "abase_service_role_key", "npg" + "_"]
    if any(token in json.dumps(c, ensure_ascii=False).lower() for token in forbidden_tokens):
        fail(f"Possível secret encontrado em capítulo {c.get('livro')} {c.get('capitulo')}")

for v in verses:
    if v["livro"] not in book_by_name:
        fail(f"Registro de versículo aponta para livro inexistente: {v['livro']}")
    if v["quantidadeVersiculos"] <= 0:
        fail(f"quantidadeVersiculos <= 0 em {v['livro']} {v['capitulo']}")

if errors:
    print("VALIDAÇÃO FALHOU")
    for err in errors:
        print(f"- {err}")
    raise SystemExit(1)

print("VALIDAÇÃO OK")
print(f"Livros: {len(books)}")
print(f"Capítulos: {len(chapters)}")
print(f"Versículos: {sum(v['quantidadeVersiculos'] for v in verses)}")
print(f"Antigo Testamento: {sum(b['totalVersiculos'] for b in books if b['testamento'] == 'Antigo')} versículos")
print(f"Novo Testamento: {sum(b['totalVersiculos'] for b in books if b['testamento'] == 'Novo')} versículos")
