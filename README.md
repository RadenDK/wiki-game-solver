# Async Wiki Game Solver

This is a small hobby project where I wanted to play around with async programming and try building my own solution to the Wiki Game — finding the shortest link path between two Wikipedia articles.

The goal is to start from one article and navigate through internal links to reach a target article, using the fewest possible steps.

My implementation is fairly simple and experimental. It can often find a solution within **3 links** in a reasonable amount of time. Beyond **4 links**, the runtime tends to grow too large for most cases.

This project was mainly about learning and experimenting with asynchronous code, and it was a fun challenge to tackle Wikipedia’s link structure.


