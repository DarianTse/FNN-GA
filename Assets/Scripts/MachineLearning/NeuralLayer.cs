public class NeuralLayer{

    public uint NeuronCount { get; private set; }
    public uint OutputCount { get; private set; }
    public double[,] Weights { get; private set; }
    
    public double Bias { get; set; }

    /// <summary>
    /// Create new neural layer 
    /// </summary>
    /// <param name="_neuronCount"># of Neurons in this layer</param>
    /// <param name="_outputCount"># of Neurons in next layer</param>
    public NeuralLayer(uint _neuronCount, uint _outputCount)
    {
        NeuronCount = _neuronCount;
        OutputCount = _outputCount;
        
        Weights = new double[_neuronCount, _outputCount];

        Bias = 1.0;
    }
   
    /// <summary>
    /// Calculate the outputs of the layer
    /// Output = ActivationFunc(Summation(Inputs * Weights) + bias)
    /// </summary>
    /// <param name="neurons">Values of the layer</param>
    /// <returns>Calculated outputs</returns>
    public double[] CalculateOutputs (double[] values)
    {
        if(values.Length != NeuronCount)
        {
            return null;
        }

        //Array of final outputs to next layer
        double[] outputs = new double[OutputCount];

        for(int nxtNeuronI = 0; nxtNeuronI < OutputCount; nxtNeuronI++)
        {
            for(int currNeuronI = 0; currNeuronI < NeuronCount; currNeuronI++)
            {
                //Summation
                outputs[nxtNeuronI] += (values[currNeuronI] * Weights[currNeuronI, nxtNeuronI]);
            }

            //Bias
            outputs[nxtNeuronI] += Bias;
            //Activation Func
            outputs[nxtNeuronI] = MathFunctions.Sigmoid(outputs[nxtNeuronI]);
        }
        return outputs;
    }

    /// <summary>
    /// Set Random Weights for connections between [currentLayerNeuron, nextLayerNeuron]
    /// </summary>
    public void SetRandomWeights()
    {
        for (int nxtNeuronI = 0; nxtNeuronI < OutputCount; nxtNeuronI++)
        {
            for (int currNeuronI = 0; currNeuronI < NeuronCount; currNeuronI++)
            {
                Weights[currNeuronI, nxtNeuronI] = MathFunctions.GetRandom(GlobalData.MinGeneValue, GlobalData.MaxGeneValue);
            }
        }
    }

    public string GetConnectionWeightsString()
    {
        string str = "";
        for (int x = 0; x < NeuronCount; x++)
        {
            for (int y = 0; y < OutputCount; y++)
            {
                str += "[" + x + "," + y + "]: " + Weights[x, y];
            }
            str += "\n";
        }
        return str;
    }
}
