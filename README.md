# Forward Neural Network and Genetic Algorithm (In development)

## Current Status: 

Tested to generation 140 - Looks like they are improving. Behaviour includes drastic turns and sudden stops around red orbs. 

## Introduction

2D simulation where lifeforms learn to pick up yellow orbs and avoid red orbs.

## Important Code

### Simulation.cs

Manager class for the simulation. Contains an instance of the genetic algorithm and the population of LifeForms.

Calls the following function in the Start() function to initialize everything the simulation needs
```
StartSimulation();

```
Set up simulation variables in the inspector


### WorldController.cs

Sets up the resources, spawning and detection

### LifeForm.cs

Wrapper class for GenoType.cs (Contains the chromosome (solution)) and NeuralNetwork.cs (Implements the chromosome)

### LifeformController.cs

Contains the behaviour of the LifeForm via providing inputs and retrieving outputs from the neural network. 

Inputs - closest energy orb x and y, closest danger orb x and y, current move vec x and y 

Outputs[0] - Left turn rate
Outputs[1] - Right turn rate
