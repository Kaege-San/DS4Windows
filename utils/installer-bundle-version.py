#!/usr/bin/env python3
"""Generate a Burn bundle version that orders CI and release builds together."""

from __future__ import annotations

import argparse
from datetime import date, datetime, timezone
import re


VERSION_PART_LIMIT = 65535
BUNDLE_EPOCH = date(2000, 1, 1)


def bundle_version(product_version: str, run_id: int, utc_day: date) -> str:
    """Use the product major/minor plus UTC day and repository run ID.

    Burn compares bundle versions independently of the MSI's ProductVersion.
    The date makes later-day builds newer, while the repository-wide run ID
    orders CI and release builds started on the same day.
    """
    parts = product_version.split(".")
    if len(parts) not in (2, 3, 4) or any(not part.isdecimal() for part in parts):
        raise ValueError(f"ProductVersion must contain 2 to 4 numeric parts: {product_version}")

    major, minor = (int(parts[0]), int(parts[1]))
    day_part = (utc_day - BUNDLE_EPOCH).days
    run_part = run_id % (VERSION_PART_LIMIT + 1)
    generated_parts = (major, minor, day_part, run_part)
    if any(part < 0 or part > VERSION_PART_LIMIT for part in generated_parts):
        raise ValueError("Generated bundle version parts must be between 0 and 65535")

    return ".".join(str(part) for part in generated_parts)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--product-version", required=True)
    parser.add_argument("--run-id", required=True, type=int)
    parser.add_argument(
        "--utc-date",
        type=date.fromisoformat,
        default=datetime.now(timezone.utc).date(),
        help="UTC build date in YYYY-MM-DD format (defaults to the current UTC date)",
    )
    args = parser.parse_args()

    if args.run_id < 0:
        parser.error("--run-id must be non-negative")

    try:
        print(bundle_version(args.product_version, args.run_id, args.utc_date))
    except ValueError as error:
        parser.error(str(error))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
