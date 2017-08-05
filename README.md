# HttpCircuitBreaker

[![Build Status](https://travis-ci.org/kiksen1987/HttpCircuitBreaker.svg?branch=master)](https://travis-ci.org/kiksen1987/HttpCircuitBreaker) 

This is a small project to extend the basic functionality of HttpClient with a basic Circuit Breaker.    
To create a CircuitBreakerHttpClient instance, you need CircuitBreakerConfig and CircuitBreakerTimerConfig.

CircuitBreakerConfig consists of:

* Max number of active calls. If this is exceded call's are returend with failure.
* Number of calls for open state. How many failures before opening circuit and stopping further call's.
* Numver of calls for closed state. How many success call's before returning to normal closed state.

CircuitBreakerTimerConfig consists of:

* Seconds to extend timer. When going to open state. How many seconds should call's not be allowed.
* Max seconds to extend timer. When extending open state. What is the max allowed value.
* Extend time function. Default is just adding extend timer and being constant. See sample below.

One function could be:   
~~~~~~~dotnetcore
i => (int) (i * 1.5)    
~~~~~~~

The way it works. Is when failing circuit open's with your defined seconds. Eg. 5.  
If the call's continue to fail, we extend the timer with your function.
Eg.    
1 => 5 seconds.    
2 => 5 * 1,5 = 7,5 round down to 7 seconds.     
3 => 7 * 1,5 = 10,5 round down to 10 seconds.    

We only work with whole seconds for the moment.    
You can go as crazy as you want with your function's as long they return an int value.     

Below here is a bare min. needed to get started with default paramaters.    

~~~~~~~dotnetcore
    var basicConfig = new CircuitBreakerConfig();
    var timerConfig = new CircuitBreakerTimerConfig();
    _circuitBreakerHttpClient = new CircuitBreakerHttpClient(new CircuitBreaker(basicConfig, timerConfig));
~~~~~~~

The whole idea behind this project, was my team allready had created alot of base proxies.    
We saw services being down and being slow and all kind of errors, which affected our services.    
Therefor I set out to create a new base CircuitBreakerHttpClient build ontop of HttpClient, so the work needed    
to implement it would be minimal.


# SimplyWeb

A small sample project of how people could work with CircuitBreakerHttpClient.    
In this sample I have made a DogApiProxy which is using the CircuitBreakerHttpClient.    
Just add the DogApiProxy to the Service Container. Then we can use dependency injection to get our instance.    
It's important that we are using some kind of Singleton pattern, to ensure we don't new CircuitBreakerHttpClient every time.    
That would ruin the whole idea of CircuitBreaker's, cause the state would be reset every time a new instance is created.    
This is just a small sample project. Use whatever framework that floats your boat. This sample just show the build in    
which comes with new basic asp net core project.

# TCB

This is where all the Circuit Breaker logic resides.   
Also this project which is build into a nuget package.    
In future there will be renaming of namespaces.

# UnitTest

I've started to add some basic unittests, to show that CircuitBreakerHttpClient return's same value as normal HttpClient.    
Will be adding more tests in the future. Like testing circuit breaker states and so on.    