from __future__ import annotations

import re
from pathlib import Path
import pdfplumber

ROOT = Path(__file__).resolve().parents[1]
PDF_PATH = next(path for path in ROOT.rglob("*.pdf") if "300" in path.name)
OUTPUT_PATH = ROOT / "Assets" / "Scripts" / "Data" / "CatEncyclopediaTable.cs"
ATTACK_BALANCE_DIVISOR = 3

def clean(value: str | None) -> str:
    return re.sub(r"\s+", " ", value or "").strip()

def number(value: str | None) -> int:
    digits = re.sub(r"[^0-9]", "", value or "")
    if not digits:
        raise ValueError(f"Invalid numeric cell: {value!r}")
    return int(digits)

def cs(value: str) -> str:
    return value.replace("\\", "\\\\").replace('"', '\\"')

raw_records: list[list[str]] = []
with pdfplumber.open(PDF_PATH) as pdf:
    for page in pdf.pages:
        for table in page.extract_tables():
            for row in table:
                if not row or len(row) != 7:
                    continue
                first = clean(row[0])
                if re.fullmatch(r"\d+", first):
                    raw_records.append([cell or "" for cell in row])
                elif not first and raw_records and any(clean(cell) for cell in row[1:]):
                    previous = raw_records[-1]
                    for index in range(1, 7):
                        continuation = row[index] or ""
                        if continuation:
                            previous[index] = f"{previous[index]}\n{continuation}" if previous[index] else continuation

records: list[dict[str, object]] = []
for row in raw_records:
    attack = clean(row[4])
    records.append({
        "id": int(clean(row[0])),
        "name": clean(row[1]),
        "hp": number(row[2]),
        "atk": max(1, number(row[3]) // ATTACK_BALANCE_DIVISOR),
        "primary": attack.split(",", 1)[0].strip(),
        "attack": attack,
        "defense": clean(row[5]),
        "debuff": clean(row[6]),
    })

ids = [int(record["id"]) for record in records]
if ids != list(range(1, 301)):
    raise ValueError(f"Expected consecutive IDs 1..300, got {ids}")
if any(int(records[index]["hp"]) < int(records[index - 1]["hp"]) for index in range(1, len(records))):
    raise ValueError("HP values must be monotonically non-decreasing after continuation rows are merged")
if any(not re.search(r"[가-힣]", str(record["name"])) for record in records):
    raise ValueError("Korean PDF extraction is corrupted; refusing to overwrite the encyclopedia table")

lines = [
    "using System.Collections.Generic;",
    "",
    "namespace PocketRoguelike",
    "{",
    "    public readonly struct CatEncyclopediaEntry",
    "    {",
    "        public int Id { get; }",
    "        public string KoreanName { get; }",
    "        public int Hp { get; }",
    "        public int Atk { get; }",
    "        public string PrimarySkillKorean { get; }",
    "        public string AttackSkillsKorean { get; }",
    "        public string DefenseSkillKorean { get; }",
    "        public string DebuffSkillKorean { get; }",
    "",
    "        public CatEncyclopediaEntry(int id, string koreanName, int hp, int atk, string primarySkillKorean, string attackSkillsKorean, string defenseSkillKorean, string debuffSkillKorean)",
    "        {",
    "            Id = id;",
    "            KoreanName = koreanName;",
    "            Hp = hp;",
    "            Atk = atk;",
    "            PrimarySkillKorean = primarySkillKorean;",
    "            AttackSkillsKorean = attackSkillsKorean;",
    "            DefenseSkillKorean = defenseSkillKorean;",
    "            DebuffSkillKorean = debuffSkillKorean;",
    "        }",
    "    }",
    "",
    "    public static class CatEncyclopediaTable",
    "    {",
    f"        public const int AttackBalanceDivisor = {ATTACK_BALANCE_DIVISOR};",
    "",
    "        private static readonly CatEncyclopediaEntry[] AllEntries =",
    "        {",
]
for r in records:
    lines.append(
        f'            new CatEncyclopediaEntry({r["id"]}, "{cs(str(r["name"]))}", {r["hp"]}, {r["atk"]}, "{cs(str(r["primary"]))}", "{cs(str(r["attack"]))}", "{cs(str(r["defense"]))}", "{cs(str(r["debuff"]))}"),'
    )
lines.extend([
    "        };",
    "",
    "        private static readonly Dictionary<int, CatEncyclopediaEntry> ById = BuildIndex();",
    "        public static IReadOnlyList<CatEncyclopediaEntry> Entries => AllEntries;",
    "",
    "        public static bool TryGet(int id, out CatEncyclopediaEntry entry) => ById.TryGetValue(id, out entry);",
    "",
    "        public static CatEncyclopediaEntry Get(int id)",
    "        {",
    "            if (!ById.TryGetValue(id, out CatEncyclopediaEntry entry))",
    "                throw new KeyNotFoundException($\"Cat encyclopedia ID {id} was not found.\");",
    "            return entry;",
    "        }",
    "",
    "        private static Dictionary<int, CatEncyclopediaEntry> BuildIndex()",
    "        {",
    "            Dictionary<int, CatEncyclopediaEntry> result = new Dictionary<int, CatEncyclopediaEntry>(AllEntries.Length);",
    "            foreach (CatEncyclopediaEntry entry in AllEntries) result.Add(entry.Id, entry);",
    "            return result;",
    "        }",
    "    }",
    "}",
])
OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
OUTPUT_PATH.write_text("\n".join(lines) + "\n", encoding="utf-8")
print(f"Generated {len(records)} records at {OUTPUT_PATH}")
