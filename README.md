# Fractalis
Traceur de fractales de type Mandelbrot et Julia
---

Est-il encore besoin de présenter les ensembles fractals de Mandelbrot ? à partir de l'équation Z -> Z<sup>2</sup> + C, on peut créer une infinité d'images fantastiques, qui possèdent des propriétés d'autosimilarité et d'invariance d'échelle : Z et C étant des nombres complexes, on compte combien d'itérations sont nécessaires pour sortir du cercle unitaire, c'est tout !

Et pour les ensembles de Julia, c'est la même formule que l'ensemble de Mandelbrot, sauf que ce n'est plus la constante C qui est liée aux pixels mais la première valeur de la variable Z (qui n'est donc plus initialisée à 0), et chaque constante C définie un ensemble de Julia particulier.

Mode d'emploi : zoomer sur l'image en sélectionnant une zone :-)

## Table des matières
- [Description](#description)
- [Exemples de vidéos](#exemples-de-vidéos)
- [Limitations](#limitations)
- [Projets](#projets)
- [Versions](#versions)
- [Liens](#liens)

## Description
Au début, j'avais imaginé optimiser le tracé avec l'algorithme des [QuadTree](https://fr.wikipedia.org/wiki/Quadtree) (arborescence en carrés de résolution progressive pour ne pas examiner tous les pixels d'un gros pavé si le nombre d'itération trouvé est le même aux quatre coins et au centre du pavé), ou alors avec un algorithme de remplissage (les fractales de Mandelbrot et Julia semblent être toujours connexes, on ne trouve pas de zone isolée a priori), et les deux algorithmes en même temps, mais en trouvant un [code source](https://www.codeproject.com/Articles/38514/The-beauty-of-fractals-A-simple-fractal-rendering "The beauty of fractals - A simple fractal rendering program done in C#") franchement plus rapide, tout cela est devenu inutile tellement la performance était supérieure !

Il est possible de produire des vidéos, mais pour le moment cela se fait par le code, il n'y a pas encore d'interface dédiée. Par exemple si un zoom vous semble particulièrement intéressant, il suffit de noter la coordonnée du centre de l'image et de réinitialiser le zoom : ensuite la progression du zoom ne changera pas la cible choisie.

## Exemples de vidéos
- [Mandelbrot01-1280x720](https://www.tiktok.com/@patrice.dargenton/video/7404797115844578593)
- [Julia01-1280x720](https://www.tiktok.com/@patrice.dargenton/video/7404795503143144736)
- [Julia02-1280x720](https://www.tiktok.com/@patrice.dargenton/video/7404794415127989537)
- [Julia02-Extrait-1280x720-30fps](https://www.tiktok.com/@patrice.dargenton/video/7404792831589535008)

## Limitations
- Il faudrait un package nuget pour la création de vidéo, ce serait plus simple.


## Projets
- Éditeur de scénario pour les vidéos ;
- Version html en Blazor.

## Versions

Voir le [Changelog.md](Changelog.md)

## Liens

Documentation d'origine complète : [Fractalis : index.html](http://patrice.dargenton.free.fr/fractal/index.html)