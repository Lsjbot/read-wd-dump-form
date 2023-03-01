# read-wd-dump-form
Application to read and analyze Wikidata dump files

This application will read a standard data dump from Wikidata, and perform various tasks mainly concerning cross-linguistic semantic analysis.

Tasks:
- Create subdump from full dump, either random fraction or selected area (all items about cities, persons, etc.)
- Create matrix with which items have Wikipedia articles in which languages
- Calculate domain coverage per language and semantic domain
- Create distance matrix between languages
- Several methods for building trees or clusters from distance matrices
- Extract various statistics on personal names
