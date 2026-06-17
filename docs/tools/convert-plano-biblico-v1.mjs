#!/usr/bin/env node
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const input = path.join(__dirname, '..', 'examples', 'plano-biblico-v1-600-dias.json');
const rows = JSON.parse(fs.readFileSync(input, 'utf8'));

const count = (predicate) => rows.filter(predicate).length;
const isMeditation = (row) => row.leituraTexto === 'Meditação livre';
const hasInterval = (row) => /\d\s*-\s*\d/.test(row.leituraTexto);
const hasException = (row) => /exceto/i.test(row.leituraTexto);

const summary = {
  totalDias: rows.length,
  meditacaoLivre: count(isMeditation),
  comIntervaloCapitulos: count(hasInterval),
  comExcecao: count(hasException),
  primeiroDia: rows[0],
  ultimoDia: rows[rows.length - 1]
};

console.log(JSON.stringify(summary, null, 2));
