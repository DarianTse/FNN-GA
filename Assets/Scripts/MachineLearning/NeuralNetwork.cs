using System.Linq;

public class NeuralNetwork{

    //Individual layers
    public NeuralLayer[] Layers { get; private set; }

    //Number of neurons in each layer
    public uint[] NetworkLayerSize { get; private set; }

    //Number of connections in the network
    public uint WeightCount { get; private set; }

    public NeuralNetwork(params uint[] layerSizes)
    {
        NetworkLayerSize = layerSizes;

        //Calculate number of connections
        for (int i = 0; i < layerSizes.Length - 1; i++)
        {
            WeightCount += (layerSizes[i] * layerSizes[i + 1]);
        }

        //Create layers. Don't create output layer because it is the output of 2nd last layer
        Layers = new NeuralLayer[layerSizes.Length - 1];
        for (int i = 0; i < Layers.Length; i++)
        {
            Layers[i] = new NeuralLayer(layerSizes[i], layerSizes[i + 1]);
        }
    }

    /// <summary>
    /// Calculate outputs of the network using current weights
    /// </summary>
    /// <param name="inputNeurons">The input layer values</param>
    /// <returns>Outputs</returns>
    public double[] CalculateOutputs(double[] inputLayerValues)
    {
        if(inputLayerValues.Length != Layers[0].NeuronCount)
        {
            return null;
        }

        //Outputs of first layer is the input layer values
        double[] outputs = inputLayerValues;
        //Get the new outputs using previous layer outputs in calculatiion
        Layers.ToList().ForEach(layer => outputs = layer.CalculateOutputs(outputs));

        return outputs;
    }

    /// <summary>
    /// Set random weights for the network
    /// </summary>
    public void SetRandomWeights()
    {
        Layers.ToList().ForEach(layer => layer.SetRandomWeights());
    }

    public string GetNetworkString()
    {
        string str = "";
        for (int i = 0; i < Layers.Length; i++)
        {
            str += "Layer: " + i + "\n" + Layers[i].GetConnectionWeightsString();
        }
        return str;
    }
}
