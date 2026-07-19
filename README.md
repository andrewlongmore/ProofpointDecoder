# ProofpointDecoder

C# port of python URLDefenseDecoder (see [help.proofpoint.com](https://help.proofpoint.com/Threat_Insight_Dashboard/Concepts/How_do_I_decode_a_rewritten_URL%3F)).
The python code is GPLv3 (i.e. copy-left), and so this code necessarily is too.

## Usage

```dos
ProofpointDecoder\bin\Release\net10.0\ProofpointDecoder.exe ^
  "https://urldefense.proofpoint.com/v1/url?u=http://www.example.com&k=0"
```
