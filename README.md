# ExternalMergeSorting

Example parameters and usage:

1. -m 0 -p "C:\\Users\WeclawskiAndrzej\source\repos\ExternalMergeSort\input2GBduplicates" -s 2000 -r 100 
   
   Generate 2GB file at absolute path with high amount of repetitive records
    
2. -m 0 -p "..\anotherFileLarge" -s 50000 -l 4 -e 300 
   
   Generate 50GB file at relative path with line length between 4 and 300 characters
    
3. -m 1 -p "C:\\Users\WeclawskiAndrzej\source\repos\ExternalMergeSort\input2GBduplicates" -d true
  
  Sort file generated at 1. with default settings but leave the temp files to be able to see what it generated during it's processing
    
4. -m 1 -p "C:\\Users\WeclawskiAndrzej\source\repos\ExternalMergeSort\input2GBduplicates" -k 20
   
   Sort file generated at 1. to see how higher amount of files merged at once affects performance
    
5. -m 1 -p "..\anotherFileLarge" -c 1000000
   
   Sort file generated at 2. with bigger chunks during split (will take up more memory but for large input files it will improve performance significantly) 
