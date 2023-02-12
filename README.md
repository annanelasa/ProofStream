# ProofStream

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
