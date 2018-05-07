# Compression-Tool
A data compression tool using the deflate scheme for text compression.

# Compression/Decompression scheme
We have decided to use a two-stage compression scheme called “Deflate”. The scheme consists of two series compression layers that starts by applying a dictionary type encoding layer using LZ77 (Lempel - Ziv 1977) and then follows an entropy type encoding layer using Huffman. We have made various modifications to the original scheme that proved to achieve a higher compression ratio and a reduced compression time. Follows a detailed discussion of our modified scheme.

# Compression algorithm 
## 1- Converting UTF-8 encoded characters to bytes
The process starts by reading the to-be-compressed file as plain UTF-8 encoded text. And then each distinct character is mapped to a value ranging from 0 to 179. These 180 different values allows the tool to compress any text written in standard Arabic, Arabic diacritics, lower and uppercase latin letters, Arabic and English numerals, control keys and special
characters.

## 2- LZ77 encoding
The second step of the algorithm, the byte stream that’s obtained from the previous step is compressed using the LZ77 algorithm. The LZ77 algorithm divides the input stream into 2 buffers, namely, search buffer and lookahead buffer. Then it parses the input stream using a sliding window technique to achieve compression by replacing repeated occurrences of data by references to a single copy of that data existing in the previously processed data stream.

When a match is found, the repeated occurrence is replaced by a reference that consists of two numbers, namely, the backward distance and the match-length. The algorithms favors longer matches and closer backward distances.

We decided to use a search buffer of size 512 KB, and thus the backward distance is encoded using 19 bits to represent values in range [1, 524288], but shifted to range [0, 524287] to fit into 19 bits. We have also decided to use a lookahead buffer of size 259 bytes, thus a match-length is encoded using 8 bits to represent values in range [4-259] but shifted to range [0-255] to fit into a byte.

We decided to only encode matches of length greater than 4 bytes. A justification for such a decision can be easily reasoned from the size of the distance-length pair we are using, which is 19 bits for the backward distance and 8 bits for the match-length, so a total of 27 bits. So if we decided to encode any matches of length less than 4, we will in fact increase the size of the file instead of compressing it. I.e. If we compressed matches of length 3, the reference distance-length pair will occupy 27 bits, while the uncompressed stream will have occupied only 24 bits, so it’s more efficient to leave it uncompressed.

#### The modifications we applied to the original LZ77 algorithm can be summarized in the following points:
- Increased search buffer size (from 32 KB to 512 KB).
- Increasing the buffer size introduced a problem in searching for the longest matches in the search buffer because of its really large size, as linear search takes a lot of time, we developed a data structure that’s basically a hashtable storing all substrings of length 4 as keys, and for each key, it stores a list of indices to where this substring starts in the search buffer. This data structure was found to greatly accelerate the compression process, reducing the compression time from an average of 13 mins per file, to an average of 10 secs per file!
- Using a flag bit to differentiate between compressed and uncompressed
sequences instead of using the mismatch byte to save space. We later found out that this modification was introduced by Storer and Szymanski, developing a new version of LZ77 called LZSS (Lempel - Ziv - Storer - Szymanski).

The output bit stream is then passed into a bit reduction phase where each group of 8 bits is packed into a byte, and a padding is added to the last group of bits if it’s size is less than 8 bits to complete a byte. The padding takes a value in a range of [0-7] and is transmitted at the very beginning of the LZ77 compressed file.

##### The output compressed stream of this layer takes the following format:
[Padding value] [Literal(s) (8 bits)]* … [Backward distance (19 bits)]* [Match length (8 bits)] … [Literal(s) (8 bits)]* … etc.

##### The output stream of this phase is 70% smaller than the original file, achieving a compression ratio of ~3.0.

# 3- Canonical Huffman encoding
The partially compressed stream obtained from the previous compression layer is then passed into a special class that parses it and extracts 3 streams, namely, the literals stream, the match-length stream, the backward-distance stream and the padding value.

#### Literals stream: 
A stream consisting of all unrepeated literal values (The values that no
matches were found for) which are left uncompressed. Values range from [0-179].

#### Match-length stream: 
A stream consisting of all the match-length values extracted from the
reference pairs. Values ranges from [4-259], but shifted to range [0-255] to fit in a byte.

#### Backward-distance stream: 
A stream consisting of all backward distance values extracted from the reference pairs. Here we decided to only encode the the most significant 8 bits of the backward distance value instead of encoding all 19 bits to reduce the header size and accelerate the encoding process. Values range from [0-255].

Each of the three streams mentioned above is considered to compose a different alphabet. Each alphabet is sent into a special probability class that calculates the number of occurrences of each value. Then a Huffman encoder is used to construct the variable length codeword for each value in these alphabets.

#### The modifications we applied to the original Huffman algorithm can be summarized in the following points:
- To reduce the header size, we decided to use a variation of Huffman encoding called the canonical form. Using the canonical codewords instead of the normal codewords allows us to only send the length of the codeword of each character in the encoded alphabet instead of sending the entire Huffman tree in the encoded file. The decoder can apply a simple algorithm that allows it to infer the entire canonical codebook from the codeword lengths sent in the encoded file.
- We calculate the number of occurrences of each character instead of its
probability to avoid precision related errors. The Huffman encoder proceeds by replacing each occurrence of a value with its corresponding canonical codeword. And then we apply the same bit reduction phase used in the LZ77 layer to pack the bit stream into compressed bytes.

##### The final compressed file takes the following format:
[180 value representing the codeword lengths of all 180 symbols] 
[256 value representing the codeword lengths of all 256 backward distances values] 
[256 value representing the codeword lengths of all 256 match length values] 
[Padding value] 
[encoded stream bytes]

### The output stream of this phase is 67% smaller than the output of the LZ77 phase and 85% smaller than the original file, achieving an average [compression ratio of ~4.31]. The folder containing the entire dataset (20 files) is reduced from 38,065,903 bytes (38 MB) to 8,841,139 bytes (8.8 MB).

##### Compressing the entire dataset takes about 4 mins, and the decompression process takes about 9 mins.

# Differences between our scheme and deflate’s
Our proposed scheme compresses the entire file in one block instead of dividing it into several blocks. After some trials we found out that dividing the files into blocks makes codewords shorter, but the gain achieved is diminished by the repeated headers (One header for each block 692 bytes each).
- While applying Huffman encoding, we consider literals, lengths, distances as 3 distinct alphabets instead of merging the literals and lengths in one alphabet. After some trials, we figured out that Huffman performs better on data that has natural repetitions (some characters are more frequent than others), such natural repetitions appear in valid linguistic text, but the match-lengths alphabet is quite random, so the introduction of the match-lengths alphabet to the literals alphabet biased the natural repetitions and resulted in codewords of approximately equal lengths, and thus little compression.
- We eliminated the use of extra bits (selection bits) by separating the stream into 3 different alphabets each taking a full range of values that can be identified without using any selection bits.
