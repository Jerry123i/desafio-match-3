# Match-3

Desafio de programação da Gazeus Games

Realizado por Igor Santos Carneiro

# OBSERVAÇÕES GERAIS
É importante notar que as alterações no projeto foram realizadas tendo em mente demonstrar proficiência com o código e experiência em desenvolvimento Unity. O produto final não apresenta um conceito fechado de game design, mas uma coleção de mecânicas e ferramentas que podem ser expandidas para uma variedade de experiências diferentes.

# FUNCIONAMENTO DO JOGO
O jogo possui 4 modos demonstrando as condições do desafio e propostas de bônus.

## 1 - STANDARD
O jogador deve combinar linhas de 3 ou mais peças para obter pontos. Combinar 4 ou mais peças da mesma cor fornece um bônus de eliminação de linhas extras, o bônus varia de acordo com a cor eliminada.  
O jogador tem acesso a uma dica que demonstra uma peça que pode ser usada em um movimento válido apertando o botão de dica. Caso o jogador passe alguns segundos sem usar um item ou fazer um movimento, a dica é fornecida automaticamente.

## 2 - ROTATÓRIO
O jogador deve realizar movimentos de rotação de 4 quadrados por vez para fazer combinações de 3 peças. Os efeitos especiais da fase 1 ainda funcionam.  
O jogador possui 2 botões para alternar entre movimentos horários ou anti-horários ao clicar nas peças, e também pode alternar essa configuração com as teclas **Q** e **E** do teclado.  
O movimento é livre nesse modo, não sendo desfeito caso não resulte em uma combinação, e o jogador não tem acesso a dicas.

## 3 - MATCH 4
Semelhante ao modo **STANDARD**, mas o jogador deve fazer combinações de quadrados de 4 peças.  
O jogador pode usar dicas nessa fase, e a criação inicial do tabuleiro também foi adaptada para garantir que o jogo não comece com quadrados já formados.

## 4 - PROCURE O PADRÃO
O jogador é apresentado a um padrão de peças diferentes que pode ser formado com 1 movimento no tabuleiro, e deve montar esse padrão.  
Quando o padrão é montado, as peças são destruídas e um novo padrão é fornecido.

# ITENS
As fases 1, 2 e 3 permitem que o jogador utilize diferentes itens, selecionando-os na barra lateral e clicando no tabuleiro para aplicar seu efeito em um ponto específico.

### 1 - PICARETA
Destrói uma única peça selecionada.

### 2 - BOMBA
Destrói várias peças em um raio centrado na peça selecionada.

### 3 - TROCA LIVRE
Permite que o jogador troque quaisquer duas peças, independente da distância.

### 4 - TERREMOTO
Destrói a camada mais baixa do tabuleiro em colunas aleatórias, porém gera peças de bloqueio que só podem ser destruídas com itens ou combinações especiais.

### 5 - SLIDERS
Movimentam todas as peças uma casa para a direita ou esquerda, podendo ser usados para formar combinações com peças do outro lado do tabuleiro.

# SOBRE AS ALTERAÇÕES DO CÓDIGO
Dentre as alterações mais relevantes para desenvolver as novas mecânicas e ferramentas, destacam-se:

- **Classe Table<T>**  
  Classe utilizada para substituir as várias listas de listas existentes no projeto.  
  `Table` utiliza um único array, mas seus valores ainda são acessados a partir de um valor de **x** e **y**, facilitando a refatoração e permitindo uma melhor legibilidade do código.

- **Classe MatchInformation**  
  Utilizada para armazenar informações sobre tipos e posição das combinações de tiles desejadas pelo jogo. Foi essencial para desenvolver efeitos como os especiais ao combinar 4 tiles da mesma cor, e poderia facilmente ser usada para responder a outras ações do jogador, como fazer um **match** em cruz ou 2 **matches** simultâneos em paralelo.

- **Refatoração da função SwapTiles**  
  A função `SwapTiles`, responsável por retornar a lista de `BoardSequences` para as animações, foi refatorada para uma versão mais genérica que recebe instruções de como mover as peças, quais critérios usar para deletar peças e quais critérios usar para gerar novas peças.  
  Essa refatoração torna o código do jogo bastante flexível, facilitando a criação de novas formas de jogar e efeitos especiais.

# KNOWN ISSUES
Os quatro modos de jogo parecem funcionar sem maiores problemas, porém existem alguns pontos de código e usabilidade que poderiam ser melhorados dado mais tempo.

- O modo de jogo de rotação não possui as mecânicas de dica ou de validação de movimento.  
  A estrutura atual do código consegue lidar bem com novas regras de destruição sendo verificadas, porém não foi explorado a fundo novas formas de movimento sendo usadas como **core mechanic**.

- Para maior agilidade e legibilidade do código, o script que coleta movimentos válidos para dica da combinação de 3 peças não detecta combinações em potencial nas extremas bordas do tabuleiro.  
  O funcionamento atual do script foi considerado suficiente para o caso de uso do jogo.

- O modo de jogo **PROCURE O PADRÃO** ocasionalmente pede por um padrão que já existe no tabuleiro, geralmente resultando de procurar um padrão que seria criado através de uma troca de dois tiles da mesma cor.

- A classe **MatchInformation** poderia estar melhor organizada para lidar com o modo de jogo **PROCURE O PADRÃO**.  
  Com mais tempo, poderiam ser exploradas soluções utilizando variações de classes herdadas de `MatchInformation`.

- O jogo pode atingir um estado sem combinações válidas.  
  No projeto atual isso não é um problema, pois o jogador possui uso livre de diversos itens.  
  Os casos em que isso acontece já são observados pelo script que procura sugestões de movimento. Bastaria desenvolver uma função que embaralhasse as peças quando a situação fosse detectada.


Developed in Unity 2022.3.62f1

## Links
- https://unity.com/releases/editor/archive
