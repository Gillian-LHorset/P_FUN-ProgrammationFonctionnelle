# Évaluation intermédiaire (80 %) – Gillian L'Horset

> Mise à jour du 16.10.2026 : les méthodes d'extension ne sont **pas exigées** (notion non vue en cours), même si le CdC les mentionne.

---

## En bref

Bonne démarche de gestion de projet (JdT soigné et commenté, US illustrées, bilan de planification honnête) et une idée intéressante (base SQLite, gestion des doublons, légende colorée). **Mais le produit est encore loin du CdC** : jeu de données insuffisant (2 séries de 61 et 125 valeurs), séries stockées non réaffichées au démarrage, plusieurs éléments d'interface non fonctionnels, aucun test. De plus, la première importation risque de planter sur une machine vierge (voir risques).

## Axes de travail prioritaires pour la finalisation

1. **Corriger le schéma de base de données** : ajouter une entité `DataSet` au modèle et tester l'application depuis une base vide.
2. **Recharger les séries stockées au démarrage** et afficher le vrai titre du jeu de données dans la légende (remplacer `"tempString"`, cf. TODO).
3. **Compléter le jeu de données** : au moins 5 stations (ou 5 grandeurs) avec ≥ 500 valeurs chacune, par exemple des valeurs mensuelles plutôt qu'annuelles, et expliquer leur cohérence.
4. **Ajouter un projet de tests** (≥ 3 tests : parsing CSV valide, CSV invalide → `null`, détection des doublons) et remplacer le `foreach` de `DataSeries.FromCsv` par un `Select`.
5. Rendre fonctionnels (ou retirer) les champs de dates et le menu « Changement de graphique ».
6. **Compléter le rapport** : sources et cohérence des données, rapport de tests, usage de l'IA (≥ 200 mots, exemples concrets), bilan produit, conclusion ; aligner l'US d'import (JSON → CSV).
