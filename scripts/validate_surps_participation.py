#!/usr/bin/env python3
"""Validate Economy's local SURPS sourcebook declaration."""
from __future__ import annotations

import hashlib
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / ".surps" / "participation.json"


def fail(errors: list[str], message: str) -> None:
    errors.append(message)


def read_json(path: Path, errors: list[str]) -> dict | None:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, UnicodeError, json.JSONDecodeError) as exc:
        fail(errors, f"{path.relative_to(ROOT)}: invalid JSON: {exc}")
        return None
    if not isinstance(value, dict):
        fail(errors, f"{path.relative_to(ROOT)}: expected a JSON object")
        return None
    return value


def local_file(value: object, label: str, errors: list[str]) -> Path | None:
    if not isinstance(value, str) or not value.strip():
        fail(errors, f"{label}: expected a non-empty repository-relative path")
        return None
    path = (ROOT / value).resolve()
    if ROOT.resolve() not in path.parents:
        fail(errors, f"{label}: path escapes the repository")
        return None
    if not path.is_file():
        fail(errors, f"{label}: file does not exist: {value}")
        return None
    return path


def main() -> int:
    errors: list[str] = []
    manifest = read_json(MANIFEST, errors)
    if manifest is None:
        return 1

    if manifest.get("schemaVersion") != 1:
        fail(errors, "participation.json: schemaVersion must be 1")
    if manifest.get("repository") != "TrentBest/TheSingularityWorkshop.Economy":
        fail(errors, "participation.json: repository identity is incorrect")
    if manifest.get("participationStatus") != "integrated-not-enrolled":
        fail(errors, "participation.json: local declaration must not claim active SURPS enrollment")

    books = manifest.get("books")
    if not isinstance(books, list) or not books:
        fail(errors, "participation.json: books must be a non-empty array")
        books = []

    ids: set[str] = set()
    cover_paths: set[str] = set()
    cover_hashes: set[str] = set()
    checked = 0

    for index, book in enumerate(books):
        label = f"participation.json: books[{index}]"
        if not isinstance(book, dict):
            fail(errors, f"{label}: expected an object")
            continue

        book_id = book.get("id")
        if not isinstance(book_id, str) or not book_id.strip() or book_id in ids:
            fail(errors, f"{label}.id: must be unique and non-empty")
        else:
            ids.add(book_id)

        for field in ("title", "category", "version", "status"):
            if not isinstance(book.get(field), str) or not book[field].strip():
                fail(errors, f"{label}.{field}: must be non-empty")
        if book.get("status") not in {"proposed", "draft", "published"}:
            fail(errors, f"{label}.status: invalid publication status")
        lod = book.get("lod")
        if not isinstance(lod, int) or isinstance(lod, bool) or lod < 0:
            fail(errors, f"{label}.lod: must be a non-negative integer")

        source_path = local_file(book.get("bookPath"), f"{label}.bookPath", errors)
        descriptor_path = local_file(book.get("descriptorPath"), f"{label}.descriptorPath", errors)
        cover_path = local_file(book.get("coverPath"), f"{label}.coverPath", errors)
        if isinstance(book.get("coverPath"), str):
            if book["coverPath"] in cover_paths:
                fail(errors, f"{label}.coverPath: cover path is reused")
            cover_paths.add(book["coverPath"])
        if cover_path is not None:
            digest = hashlib.sha256(cover_path.read_bytes()).hexdigest()
            if digest in cover_hashes:
                fail(errors, f"{label}.coverPath: cover artwork duplicates another book's cover")
            cover_hashes.add(digest)
            if cover_path.suffix.lower() != ".svg":
                fail(errors, f"{label}.coverPath: cover must be SVG")

        if descriptor_path is None:
            continue
        descriptor = read_json(descriptor_path, errors)
        if descriptor is None:
            continue
        checked += 1

        required = {"schemaVersion", "id", "category", "lod", "title", "version",
                    "cover", "lodBadge", "publisher", "publisherMoniker"}
        for missing in sorted(required - descriptor.keys()):
            fail(errors, f"{book.get('descriptorPath')}: missing {missing}")
        if required - descriptor.keys():
            continue

        for field in ("id", "category", "lod", "title", "version"):
            if descriptor.get(field) != book.get(field):
                fail(errors, f"{book['descriptorPath']}: {field} differs from manifest")
        if descriptor.get("schemaVersion") != 1:
            fail(errors, f"{book['descriptorPath']}: schemaVersion must be 1")
        if descriptor.get("source") != book.get("bookPath"):
            fail(errors, f"{book['descriptorPath']}: source differs from bookPath")
        if descriptor.get("cover") != book.get("coverPath"):
            fail(errors, f"{book['descriptorPath']}: cover differs from coverPath")
        expected_badge = (
            "https://raw.githubusercontent.com/TrentBest/"
            "TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/"
            f"development/docs/assets/rulebook-lod-{lod}.svg"
        )
        if descriptor.get("lodBadge") != expected_badge:
            fail(errors, f"{book['descriptorPath']}: lodBadge must use the canonical SURPS LOD {lod} asset URL")
        moniker = descriptor.get("publisherMoniker")
        if not isinstance(moniker, str) or not moniker.startswith("https://"):
            fail(errors, f"{book['descriptorPath']}: publisherMoniker must use HTTPS")

        if source_path is not None:
            source = source_path.read_text(encoding="utf-8")
            heading = re.search(r"^# (.+)$", source, re.MULTILINE)
            if not heading or heading.group(1).strip() != book.get("title"):
                fail(errors, f"{book['bookPath']}: H1 must match the canonical title")
            if f"`{book_id}`" not in source:
                fail(errors, f"{book['bookPath']}: stable sourcebook ID is missing")
            edition_marker = "**Edition:** " + chr(96) + str(book.get("version")) + chr(96)
            if edition_marker not in source:
                fail(errors, f"{book['bookPath']}: edition must match manifest version")

    if errors:
        print("Economy SURPS sourcebook validation FAILED:")
        for error in errors:
            print(f" - {error}")
        return 1

    print(f"Economy SURPS sourcebook validation passed: {len(books)} books, {checked} descriptors.")
    print("All source IDs, canonical titles, descriptor metadata, LOD badges, and distinct covers agree.")
    print("This local check does not approve ownership claims or enroll the repository.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
