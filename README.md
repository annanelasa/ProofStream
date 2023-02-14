# ProofStream

Article
MODELS FOR GENERATION OF PROOF FOREST IN ZK-SNARK BASED SIDECHAINS
Yuri Bespalov, Lyudmila Kovalchuk , Hanna Nelasa, Roman Oliynykov, Rob Viglione

Abstract:
Sidechains are among the most promising scalability and extended functionality solutions for blockchains. Application of zero knowledge techniques (Latus, Mina) allows to reach high level security and general throughput, though it brings new challenges on keeping decentralization where significant effort is required for robust computation of zk-proofs. We consider a simultaneous decentralized creation of various zk-proof trees that form proof-trees sequences in sidechains in the model that combines behavior of provers, both deterministic (mutually consistent) or stochastic (independent) and types of proof trees. We define the concept of efficiency of such process, introduce its quantity measure and recommend parameters for tree creation. In deterministic cases, the sequences of published trees are ultimately periodic and ensure the highest possible efficiency (no collisions in proof creation). In stochastic cases, we get a universal measure of prover efficiencies given by the explicit formula in one case or calculated by a simulation model in another case. The optimal number of allowed provers’ positions for a step can be set for various sidechain parameters, such as number of provers, number of time steps within one block, etc. Benefits and restrictions for utilization of non-perfect binary proof trees are also explicitly presented. 

Keywords:
Binary tree; Perfect tree; Magma; Operad; PRO; Occupancy distribution; blockchain; sidechain; zk-SNARK; succint blockchain

The general scheme of simulation model.
A special feature our algorithm is a simultaneous construction of several trees forming a linear ordered forest, rather than a single tree. Periodically the most left tree is included in the published block and the rest are considered as a buffer for further block generation.

Input parameters are: 
• dichotomous bbehavior regulates behavior of provers, deterministic (mutually consistent) or stochastic (independent);
• dichotomous bshape describes the shape of generated trees, only perfect binary trees or arbitrary strict binary trees;
• number of blocks published during the epoch;
• number of steps for one block generation;
• number of provers;
• number of positions allocated for proof.


Launch:
ProofStream.exe 1 1 <number of steps in block> <number of provers>
ProofStream.exe 1 0 <number of blocks> <number of steps in block> <number of provers>
ProofStream.exe 0 0 <number of blocks> <number of steps in block> <number of proovers> <number of positions>                      
ProofStream.exe 0 1 <number of blocks> <number of steps in block> <number of proovers> <number of positions>


Example:
PS D:\Work\ProofStream> ./ProofStream 1 1 5 10                      
Iterations=100                                                      
bDet=True  bPrf=True                                                
nSteps=5 nProvers=10

PS D:\Work\ProofStream> ./ProofStream 1 0 200 5 10                                                      
bDet=True  bPrf=False                                                                                   
nBlocks=200 nSteps=5 nProvers=10

PS D:\Work\ProofStream> ./ProofStream 0 1 200 5 10 100              
bDet=False  bPrf=True                                               
nBlocks=200 nSteps=5 nProvers=10 nPositions_max= 100  

PS D:\Work\ProofStream> ./ProofStream 0 0 200 5 10 100              
bDet=False  bPrf=False                                              
nBlocks=200 nSteps=5 nProvers=10 nPositions_max= 100 
